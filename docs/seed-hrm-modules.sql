-- ============================================================
-- HRM Ürün Modülleri - Seed Script
-- ProductId: 20D73BF3-47BB-449D-9FEA-28DFEB3392A8
-- ============================================================

DECLARE @ProductId UNIQUEIDENTIFIER = '20D73BF3-47BB-449D-9FEA-28DFEB3392A8';
DECLARE @Now DATETIME = GETUTCDATE();

INSERT INTO [Product].[ProductModules]
    (Id, ProductId, ModuleCode, Name, Description, AdditionalPrice, CurrencyCode, IsOptional, IsActive, SortOrder, CreatedAt, IsDeleted)
VALUES
-- 1. Dashboard & Analitik
(NEWID(), @ProductId, 'DASHBOARD_ANALYTICS', N'Dashboard & Analitik',
N'Devamsızlık oranı, Geç gelme ortalaması, Erken çıkma ortalaması, Ortalama giriş/çıkış saati, Ortalama çalışma süresi, Toplam personel, Yıllık izin kullanım oranı, Zamanında giriş oranı, Günlük operasyon özeti, Haftalık özet, Yaş dağılımı, Medeni durum dağılımı, Askerlik durumları, Yaklaşan etkinlikler, Mesai özeti, İzin özeti, Masraf özeti, Ödeme özeti, İşçi geliş durumu (geç/devamsız)',
0, 'TRY', 1, 1, 1, @Now, 0),

-- 2. Personel Yönetimi
(NEWID(), @ProductId, 'EMPLOYEE_MANAGEMENT', N'Personel Yönetimi',
N'Çalışan listeleme, Tekli personel ekleme, Toplu personel ekleme, Personel kartı (genel kişisel bilgiler, acil durum kişileri, banka bilgileri, kariyer bilgileri), İşten çıkarma, Personel departman yönetimi, Personel rol yönetimi, Personel bilgi görüntüleme',
0, 'TRY', 1, 1, 2, @Now, 0),

-- 3. Organizasyon Yapısı
(NEWID(), @ProductId, 'ORGANIZATION_STRUCTURE', N'Organizasyon Yapısı',
N'Şirket yönetimi, Şube yönetimi, Departman yönetimi, Pozisyon yönetimi, Kategori yönetimi, Görev tanımları, Lokasyon yönetimi, Mesai alanları',
0, 'TRY', 1, 1, 3, @Now, 0),

-- 4. PDKS & Devam Kontrol Sistemi
(NEWID(), @ProductId, 'PDKS_ATTENDANCE', N'PDKS & Devam Kontrol Sistemi',
N'Giriş-çıkış kayıtları, Hareket kayıtları, Kapı logları, Mola hareketleri, Çalışma hareketleri, Giriş/çıkış saati düzenleme, Konum bazlı giriş/çıkış, Mobil giriş/çıkış, PDKS bilgileri, Kart işlemleri, Mesai giriş/çıkış işlemleri',
0, 'TRY', 1, 1, 4, @Now, 0),

-- 5. Vardiya & Mesai Yönetimi
(NEWID(), @ProductId, 'SHIFT_OVERTIME', N'Vardiya & Mesai Yönetimi',
N'Vardiya tipleri, Vardiya yönetimi, Mesai parametreleri, Esnek giriş/çıkış, Mesai takvimi, Fazla mesai yönetimi, Mesai beyanı, Mesai onay süreçleri',
0, 'TRY', 1, 1, 5, @Now, 0),

-- 6. İzin Yönetimi
(NEWID(), @ProductId, 'LEAVE_MANAGEMENT', N'İzin Yönetimi',
N'İzin türleri, İzin tanımları, İzin talepleri, İzin onay/red, İzin düzenleme, İzin takvimi, İzin yazdırma, Yıllık izin özetleri',
0, 'TRY', 1, 1, 6, @Now, 0),

-- 7. Bordro & Puantaj
(NEWID(), @ProductId, 'PAYROLL_TIMESHEET', N'Bordro & Puantaj',
N'Puantaj oluşturma, Puantaj yönetimi, Bordro oluşturma, Bordro görüntüleme, Bordro indirme, Bordro alıcı yönetimi, Maaş işlemleri',
0, 'TRY', 1, 1, 7, @Now, 0),

-- 8. Ödeme Yönetimi
(NEWID(), @ProductId, 'PAYMENT_MANAGEMENT', N'Ödeme Yönetimi',
N'Ödeme kayıtları, Ödeme ekleme, Ödeme düzenleme, Ödeme iptal, Ödeme görüntüleme',
0, 'TRY', 1, 1, 8, @Now, 0),

-- 9. Masraf Yönetimi
(NEWID(), @ProductId, 'EXPENSE_MANAGEMENT', N'Masraf Yönetimi',
N'Masraf talebi, Masraf ekleme, Masraf düzenleme, Masraf iptali, Masraf onay/red, Masraf ödeme onayı, Masraf kategorileri, Belge görüntüleme',
0, 'TRY', 1, 1, 9, @Now, 0),

-- 10. Belge & Doküman Yönetimi
(NEWID(), @ProductId, 'DOCUMENT_MANAGEMENT', N'Belge & Doküman Yönetimi',
N'Belge yükleme, Belge listeleme, Belge indirme, Belge silme, Personel evrakları',
0, 'TRY', 1, 1, 10, @Now, 0),

-- 11. Zimmet Yönetimi
(NEWID(), @ProductId, 'ASSET_ASSIGNMENT', N'Zimmet Yönetimi',
N'Zimmet listeleme, Zimmet ekleme, Zimmet düzenleme',
0, 'TRY', 1, 1, 11, @Now, 0),

-- 12. Eğitim Yönetimi (LMS)
(NEWID(), @ProductId, 'TRAINING_LMS', N'Eğitim Yönetimi (LMS)',
N'Eğitim tanımlama, Eğitim içerikleri, Eğitim atama, Zorunlu eğitimler, Opsiyonel eğitimler, Eğitim tamamlama takibi',
0, 'TRY', 1, 1, 12, @Now, 0),

-- 13. İşe Alım & Aday Takip Sistemi (ATS)
(NEWID(), @ProductId, 'RECRUITMENT_ATS', N'İşe Alım & Aday Takip Sistemi (ATS)',
N'İş başvuruları (listeleme, onaylama, reddetme, mülakata alma), CV havuzu (listeleme, onaylama, reddetme), Mülakat yönetimi (planlama, onaylama, reddetme), İşe alım talepleri (oluşturma, onaylama, düzenleme, reddetme), Tamamlanan işe alımlar, İşe alım raporları',
0, 'TRY', 1, 1, 13, @Now, 0),

-- 14. Stajyer Yönetimi
(NEWID(), @ProductId, 'INTERN_MANAGEMENT', N'Stajyer Yönetimi',
N'Staj başvuruları, Başvuru onay/red, Stajyer yönetimi, Staj dönemleri, Stajyer bilgi güncelleme',
0, 'TRY', 1, 1, 14, @Now, 0),

-- 15. Rapor Yönetimi
(NEWID(), @ProductId, 'REPORT_MANAGEMENT', N'Rapor Yönetimi',
N'Rapor ekleme, Rapor görüntüleme, Rapor düzenleme, Rapor iptali',
0, 'TRY', 1, 1, 15, @Now, 0),

-- 16. Disiplin & Hukuki Süreçler
(NEWID(), @ProductId, 'DISCIPLINE_LEGAL', N'Disiplin & Hukuki Süreçler',
N'Ceza türleri, Ceza kayıtları, İcra kayıtları, Ayrılış türleri',
0, 'TRY', 1, 1, 16, @Now, 0),

-- 17. Servis & Ulaşım Yönetimi
(NEWID(), @ProductId, 'TRANSPORT_MANAGEMENT', N'Servis & Ulaşım Yönetimi',
N'Araç yönetimi, Durak yönetimi, Hat yönetimi, Filtreleme',
0, 'TRY', 1, 1, 17, @Now, 0),

-- 18. Ziyaretçi Yönetimi
(NEWID(), @ProductId, 'VISITOR_MANAGEMENT', N'Ziyaretçi Yönetimi',
N'Ziyaretçi kayıtları, Ziyaretçi atama, Düzenleme, Çıkış işlemleri',
0, 'TRY', 1, 1, 18, @Now, 0),

-- 19. Cihaz & Donanım Yönetimi
(NEWID(), @ProductId, 'DEVICE_HARDWARE', N'Cihaz & Donanım Yönetimi',
N'Cihaz listeleme, Cihaz ekleme, Cihaz düzenleme, Cihaz silme, Şirket cihazları',
0, 'TRY', 1, 1, 19, @Now, 0),

-- 20. Şirket Ayarları & Sistem Yönetimi
(NEWID(), @ProductId, 'SYSTEM_SETTINGS', N'Şirket Ayarları & Sistem Yönetimi',
N'LDAP entegrasyonu, Yetkilendirme, Roller, Parametre yönetimi, Tatil günleri, Lisans yönetimi, Ana sayfa düzenleme, Hızlı erişim ayarları',
0, 'TRY', 1, 1, 20, @Now, 0),

-- 21. Çalışan Self Servis (ESS)
(NEWID(), @ProductId, 'EMPLOYEE_SELF_SERVICE', N'Çalışan Self Servis (ESS)',
N'Profil yönetimi, İzin talebi oluşturma, Masraf talebi oluşturma, Mesai beyanı oluşturma, Bordro görüntüleme, Belge yönetimi, Banka bilgileri yönetimi, Acil durum kişileri yönetimi, Mesai hareketleri görüntüleme, Bildirimler, Eksik bilgi uyarıları',
0, 'TRY', 1, 1, 21, @Now, 0);

-- Doğrulama
SELECT COUNT(*) AS InsertedModuleCount
FROM [Product].[ProductModules]
WHERE ProductId = '20D73BF3-47BB-449D-9FEA-28DFEB3392A8' AND IsDeleted = 0;
