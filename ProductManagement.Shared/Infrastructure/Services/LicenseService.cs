using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ProductManagement.Shared.Infrastructure.Extensions;
using ProductManagement.Infrastructure.Extensions;

namespace ProductManagement.Shared.Infrastructures.Services;

public class LicenseService : IDisposable
{
    private bool _disposed = false;
    
    
    public CreateLicenseDto CreateLicense(CreateLicenseModel model)
    {
        using var rsa = RSA.Create();
        rsa.KeySize = 2048;
        
        if (!model.YeniKayit)
        {
            rsa.ImportPkcs8PrivateKey(model.PrivateKey!, out _);
        }

        // PEM export (client'a göstermek için)
        string publicKeyPem = rsa.ExportPublicKeyPem();
        
        // 2. Lisans modelini oluştur (imza için)
        var licenseForSigning = new
        {
            CustomerCode = model.CustomerCode,
            ExpirationDate = model.ExpirationDate.Date,
            LicenseKey = model.LicenseKey,
            Features = model.Features,
            AddOns = model.AddOns
        };

        var signingJsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            Converters = { new Converters.DateTimeConverter() }
        };
        string licenseDataForSigning = JsonSerializer.Serialize(licenseForSigning, signingJsonOptions);

        // Dış doğrulayıcı ile aynı akış: önce payload hash'i, sonra hash byte'larını SignData ile imzala.
        byte[] signingHashBytes;
        using (var signingSha256 = SHA256.Create())
        {
            signingHashBytes = signingSha256.ComputeHash(Encoding.UTF8.GetBytes(licenseDataForSigning));
        }
        var signatureBytes = rsa.SignData(signingHashBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var signatureBase64 = Convert.ToBase64String(signatureBytes);

        // 4. Lisans JSON'unu oluştur (sadece public bilgiler)
        var licenseForFile = new ClientLicenseModel
        {
            CustomerCode = model.CustomerCode,
            ExpirationDate = model.ExpirationDate,
            LicenseKey = model.LicenseKey,
            Signature = signatureBase64,
            PublicKey = publicKeyPem,
            Features = model.Features,
            AddOns = model.AddOns
        };

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            Converters = { new Infrastructures.Converters.DateTimeConverter() }
        };
        string licenseJson = JsonSerializer.Serialize(licenseForFile, jsonOptions);

        // 5. Dosya hash'ini oluştur (private key ile imzalanmış)
        string fileHash = licenseJson.GenerateFileHash();
        byte[] hashBytes = Encoding.UTF8.GetBytes(fileHash);
        var hashSignatureBytes = rsa.SignData(hashBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var hashSignature = Convert.ToBase64String(hashSignatureBytes);

        // 6. Veritabanında saklanacak tam model
        var storedLicense = new CreateLicenseDto
        {
            CustomerCode = model.CustomerCode,
            LicenseKey = model.LicenseKey,
            ExpirationDate = model.ExpirationDate.ToString("yyyy-MM-dd"),
            
            // Client'a dönmemk için;
            PublicKeyPem = publicKeyPem,
            
            // Db'de saklamak için;
            PrivateKey = rsa.ExportPkcs8PrivateKey(),
            PublicKey = rsa.ExportSubjectPublicKeyInfo(),
            
            Signature = signatureBase64,
            FileHash = hashSignature,
            Features = model.Features,
            AddOns = model.AddOns
        };

        return storedLicense;
    }
    
    
    public LicenseValidationResult VerifyLicense(LicenseValidationModel model)
    {
        var result = new LicenseValidationResult();

        try
        {
            var validationJsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
                Converters = { new Infrastructures.Converters.DateTimeConverter() }
            };
            
            string normalizedJson = JsonSerializer.Serialize(model.FileContent, validationJsonOptions);

            // 4. Dosya hash kontrolü
            var currentFileHash = normalizedJson.GenerateFileHash();
            using var rsa = RSA.Create();
            rsa.ImportFromPem(model.PublicKeyPem.ToCharArray());

            byte[] hashBytes = Encoding.UTF8.GetBytes(currentFileHash);
            byte[] storedHashSignature = Convert.FromBase64String(model.ValidFileHash);

            result.HashValid = rsa.VerifyData(hashBytes, storedHashSignature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            if (!result.HashValid)
            {
                result.ErrorMessage = "Dosya değiştirilmiş veya bozulmuş.";
                return result;
            }

            // 5. İmza kontrolü - Hash-based validation
            var licenseForValidation = new
            {
                CustomerCode = model.FileContent.CustomerCode,
                ExpirationDate = model.FileContent.ExpirationDate,
                LicenseKey = model.FileContent.LicenseKey,
                Features = model.FileContent.Features,
                AddOns = model.FileContent.AddOns
            };

            var signatureValidationOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
                Converters = { new Converters.DateTimeConverter() }
            };
            string licenseDataForValidation = JsonSerializer.Serialize(licenseForValidation, signatureValidationOptions);

            using var validationSha256 = SHA256.Create();
            byte[] validationHashBytes = validationSha256.ComputeHash(Encoding.UTF8.GetBytes(licenseDataForValidation));
            byte[] signatureBytes = Convert.FromBase64String(model.FileContent.Signature);

            result.SignatureValid = rsa.VerifyData(validationHashBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            if (!result.SignatureValid)
            {
                result.ErrorMessage = "İmza geçersiz.";
                return result;
            }

            // 6. Expiry kontrolü - DateTime objesi zaten var
            var expiry = model.FileContent.ExpirationDate;

            result.ExpiryValid = DateTime.UtcNow.Date <= expiry.Date;
            var validExpirationDateValid = DateTime.UtcNow.Date <= model.ValidExpirationDate;

            if (!result.ExpiryValid || !validExpirationDateValid)
            {
                result.ErrorMessage = "Lisans süresi dolmuş.";
                return result;
            }

            // 6. Tüm kontroller başarılı
            result.IsValid = true;
            return result;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Doğrulama hatası: {ex.Message}";
            return result;
        }
    }
    
    ////////// OBJECT DISPOSING AREA :: BEGIN

    public void Dispose()
    {
        //Console.WriteLine("- {0} was disposed!", this.GetType().Name);
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        // Check to see if Dispose has already been called.
        if (!_disposed)
        {
            // If disposing equals true, dispose all managed
            // and unmanaged resources.
            if (disposing)
            {
                // Dispose managed resources.
                //component.Dispose();
                //Console.WriteLine("- {0} was disposing!", this.GetType().Name);
            }

            // Note disposing has been done.
            _disposed = true;
        }
    }

    ////////// OBJECT DISPOSING AREA :: END
    
}


public class CreateLicenseModel
{
    public required string CustomerCode { get; set; }
    public required string LicenseKey { get; set; }
    public required DateTime ExpirationDate { get; set; }

    // Ilk lisans ise private key olusturulacak. Lisans yenileme ise db'de kayıtlı private key kullanılacak.
    public bool YeniKayit { get; set; } = false;
    public byte[]? PrivateKey { get; set; }
    public byte[]? PublicKey { get; set; }
    
    // Lisans özellikleri (dinamik)
    public Dictionary<string, string>? Features { get; set; }
    
    // Lisans add-onları (dinamik)
    public object? AddOns { get; set; }
}

public class CreateLicenseDto
{
    public required string CustomerCode { get; set; }   
    public required string LicenseKey { get; set; }   
    public required string ExpirationDate { get; set; }   

    // Client'a dönmek için gerekli;
    public required string PublicKeyPem { get; set; }
    
    // db'de saklamak için gerekli;
    public required byte[] PublicKey { get; set; }
    public required byte[] PrivateKey { get; set; }
    
    public required string Signature { get; set; }   
    public required string FileHash { get; set; }
    public Dictionary<string, string>? Features { get; set; }
    public object? AddOns { get; set; }
}

// Client'a gönderilecek model içeriği
public class ClientLicenseModel
{
    public required string CustomerCode { get; set; }
    public required DateTime ExpirationDate { get; set; }
    public required string LicenseKey { get; set; }
    public required string Signature { get; set; }
    public required string PublicKey { get; set; }
    public Dictionary<string, string>? Features { get; set; }
    public object? AddOns { get; set; }
}


public class LicenseValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public bool ExpiryValid { get; set; }
    public bool HashValid { get; set; }
    public bool SignatureValid { get; set; }
}

public class LicenseValidationModel
{
    public required string PublicKeyPem { get; set; }
    public required string ValidFileHash { get; set; }
    public required DateTime ValidExpirationDate { get; set; }
    public required LicenseValidationModel_file FileContent { get; set; }
    
    public class LicenseValidationModel_file
    {
        public required string CustomerCode { get; set; }
        public required DateTime ExpirationDate { get; set; }
        public required string LicenseKey { get; set; }
        public required string Signature { get; set; }
        public required string PublicKey { get; set; }
        public Dictionary<string, string>? Features { get; set; }
        public object? AddOns { get; set; }
    }
}
