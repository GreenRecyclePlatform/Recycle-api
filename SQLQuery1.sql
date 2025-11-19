-- 1. Create User RecycleDB
INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, 
                         EmailConfirmed, PasswordHash, SecurityStamp, PhoneNumber, 
                         PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, 
                         AccessFailedCount, FirstName, LastName, DateOfBirth)
VALUES 
('11111111-1111-1111-1111-111111111111', 'admin@test.com', 'ADMIN@TEST.COM', 
 'admin@test.com', 'ADMIN@TEST.COM', 1, 
 'AQAAAAIAAYagAAAAEP5vW8qF4FYxVZXMK7gLqmJ6T3tKxHLKzNQy0YvqV/R5bR8tKxC9pZlN8vQK1w==', 
 NEWID(), '01000000001', 1, 0, 1, 0, 
 'Admin', 'User', '1990-01-01'),
 
('22222222-2222-2222-2222-222222222222', 'driver@test.com', 'DRIVER@TEST.COM', 
 'driver@test.com', 'DRIVER@TEST.COM', 1, 
 'AQAAAAIAAYagAAAAEP5vW8qF4FYxVZXMK7gLqmJ6T3tKxHLKzNQy0YvqV/R5bR8tKxC9pZlN8vQK1w==', 
 NEWID(), '01000000002', 1, 0, 1, 0, 
 'Mohamed', 'Ali', '1995-05-15'),
 
('33333333-3333-3333-3333-333333333333', 'customer@test.com', 'CUSTOMER@TEST.COM', 
 'customer@test.com', 'CUSTOMER@TEST.COM', 1, 
 'AQAAAAIAAYagAAAAEP5vW8qF4FYxVZXMK7gLqmJ6T3tKxHLKzNQy0YvqV/R5bR8tKxC9pZlN8vQK1w==', 
 NEWID(), '01000000003', 1, 0, 1, 0, 
 'Ahmed', 'Hassan', '1998-08-20');

-- 2. Create Driver Profile
INSERT INTO DriverProfiles (Id, UserId, profileImageUrl, idNumber, Rating, 
                           ratingCount, IsAvailable, TotalTrips, CreatedAt)
VALUES 
('22222222-2222-2222-2222-222222222222', 
 '22222222-2222-2222-2222-222222222222',
 'https://example.com/driver.jpg', '29505151234567', 
 4.5, 10, 1, 25, GETUTCDATE());

-- 3. Create Address (✅ مظبوط دلوقتي - مع Governorate)
INSERT INTO Addresses (Id, UserId, Street, City, Governorate, PostalCode)
VALUES 
('44444444-4444-4444-4444-444444444444',
 '33333333-3333-3333-3333-333333333333',
 '123 Tahrir Street', 'Cairo', 'Cairo', '11511');

-- 4. Create Pickup Request
INSERT INTO PickupRequests (RequestId, UserId, AddressId, PreferredPickupDate, 
                           Status, Notes, TotalEstimatedWeight, TotalAmount, CreatedAt)
VALUES 
('55555555-5555-5555-5555-555555555555',
 '33333333-3333-3333-3333-333333333333',
 '44444444-4444-4444-4444-444444444444',
 DATEADD(day, 2, GETUTCDATE()),
 'Pending',
 'Please collect plastic and paper',
 50.5, 150.00, GETUTCDATE());


----------------------
 INSERT INTO RequestMaterials 
( RequestId, MaterialId, EstimatedWeight, ActualWeight, PricePerKg, TotalAmount, Notes, CreatedAt)
VALUES
('a1b2c3d4-1111-2222-3333-444455556666',
 '10000000-aaaa-bbbb-cccc-000000000001',
 12.5, 11.8, 5.25, 61.95,
 'Collected without issues',
 '2025-11-18T01:00:00Z');

INSERT INTO RequestMaterials 
(Id, RequestId, MaterialId, EstimatedWeight, ActualWeight, PricePerKg, TotalAmount, Notes, CreatedAt)
VALUES
('c19e708d-9d43-4361-92ba-4d8ef87a2dd4',
 'a1b2c3d4-1111-2222-3333-444455556666',
 '10000000-aaaa-bbbb-cccc-000000000002',
 7.0, 6.4, 4.75, 30.40,
 'Wet material, weight slightly reduced',
 '2025-11-18T01:00:00Z');

INSERT INTO RequestMaterials 
(Id, RequestId, MaterialId, EstimatedWeight, ActualWeight, PricePerKg, TotalAmount, Notes, CreatedAt)
VALUES
('ea4f1e68-6c61-4f27-8b61-4ebdfaa9639a',
 'b2c3d4e5-2222-3333-4444-555566667777',
 '10000000-aaaa-bbbb-cccc-000000000003',
 20.0, 19.5, 3.60, 70.20,
 'Heavy load',
 '2025-11-18T01:00:00Z');


-- 5. Verify Data
SELECT * FROM AspNetUsers;
SELECT * FROM DriverProfiles;
SELECT * FROM Addresses;
SELECT * FROM PickupRequests;