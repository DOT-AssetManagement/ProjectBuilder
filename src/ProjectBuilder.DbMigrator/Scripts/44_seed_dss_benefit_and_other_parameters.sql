SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_dss_BenefitParameters)
BEGIN
    INSERT INTO dbo.tbl_dss_BenefitParameters (Family, Code, Description, Val)
    VALUES
        ('Flood', 0, 'No Improvement', 0.0),
        ('Flood', 1, 'Moderate Improvement', 0.01),
        ('Flood', 2, 'Substantial Improvement', 0.02),
        ('OperatingCost', 0, 'No Improvement', 0.0),
        ('OperatingCost', 1, 'Moderate Improvement', 0.05000000000000000),
        ('OperatingCost', 2, 'Substantial Improvement', 0.10000000000000000),
        ('Safety', 0, 'No Improvement', 0.0),
        ('Safety', 1, 'Moderate Improvement', 0.5),
        ('Safety', 2, 'Substantial Improvement', 1.0),
        ('TravelTime', 0, 'No Improvement', 0.0),
        ('TravelTime', 1, 'Moderate Improvement', 0.05000000000000000),
        ('TravelTime', 2, 'Substantial Improvement', 0.10000000000000000);

    PRINT 'tbl_dss_BenefitParameters seeded: ' + CONVERT(VARCHAR(10), @@ROWCOUNT) + ' rows.';
END
ELSE
    PRINT 'tbl_dss_BenefitParameters already has data -- skipped.';

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_dss_OtherParameters)
BEGIN
    INSERT INTO dbo.tbl_dss_OtherParameters (Code, Description, Val)
    VALUES
        ('B30', 'Value of time - auto ($/hr)', 13.649999618530273),
        ('B31', 'Value of time - truck ($/hr)', 31.399999618530273),
        ('B32', 'Operating cost - auto ($/mile)', 0.40999999642372131),
        ('B33', 'Operating cost - truck ($/mile)', 0.62999999523162842),
        ('B34', 'Transit use benefit ($/year)', 50000.0),
        ('B35', 'Feet per mile', 5280.0),
        ('B36', 'Bike improvement radius (mi)', 1.5),
        ('B37', 'Adult population fraction', 0.80000001192092896),
        ('B38', 'Children cyclist fraction', 0.05000000074505806),
        ('B39', 'New cyclist multiplier', 0.30000001192092896),
        ('B40', 'New cyclist benefit ($/cyclist/year)', 1400.0),
        ('B41', 'Sidewalk benefit ($/ft/year)', 1.0),
        ('B42', 'Accessible destination benefit ($/destination/vehicle/year)', 5.0),
        ('B43', 'Disadvantaged area multiplier', 2.0),
        ('B44', 'Days per year', 365.0),
        ('B45', 'Wetland restoration benefit ($/acre/year)', 20570.0),
        ('B46', 'Wildlife crossing benefit  ($/crossing/year)', 50000.0),
        ('B47', 'Fuel reduction benefit ($/gallon)', 4.0),
        ('B48', 'Pavement benefit', 1.0),
        ('B49', 'Bridge benefit', 1.0),
        ('B50', 'Culvert benefit', 1.0),
        ('B51', 'Guardrail benefit', 1.0),
        ('B52', 'Geotech benefit', 1.0),
        ('B53', 'Facilities benefit', 1.0),
        ('B54', 'Sidewalk benefit', 1.0);

    PRINT 'tbl_dss_OtherParameters seeded: ' + CONVERT(VARCHAR(10), @@ROWCOUNT) + ' rows.';
END
ELSE
    PRINT 'tbl_dss_OtherParameters already has data -- skipped.';

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_dss_AssetTypeParametersData)
BEGIN
    INSERT INTO dbo.tbl_dss_AssetTypeParametersData (AssetType, BenefitScalingFactor)
    VALUES
        ('P', 1.0),
        ('B', 1.0);

    PRINT 'tbl_dss_AssetTypeParametersData seeded: ' + CONVERT(VARCHAR(10), @@ROWCOUNT) + ' rows.';
END
ELSE
    PRINT 'tbl_dss_AssetTypeParametersData already has data -- skipped.';
