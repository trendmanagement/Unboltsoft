CREATE PROCEDURE [cqgdb].[SPOD]
	@optionname VARCHAR(45),
	@datetime DATE,
    @price FLOAT(53),
    @impliedvol FLOAT(53),
    @timetoexpinyears FLOAT(53)
AS

SET NOCOUNT ON;

MERGE INTO cqgdb.tbloptiondata as tgt_tbloptiondata
USING
    (SELECT * FROM [cqgdb].tbloptions WHERE optionname = @optionname)
    AS src_tbloptions
    ON src_tbloptions.idoption = tgt_tbloptiondata.idoption
    AND tgt_tbloptiondata.datetime = @datetime
	AND tgt_tbloptiondata.price = @price

WHEN MATCHED THEN
UPDATE
    SET 
    price = @price,
    impliedvol = @impliedvol,
    timetoexpinyears = @timetoexpinyears

WHEN NOT MATCHED THEN

    INSERT 
    (idoption, datetime, price, impliedvol,timetoexpinyears)
    VALUES 
    (src_tbloptions.idoption, @datetime, @price, @impliedvol, @timetoexpinyears);

SET NOCOUNT OFF;