CREATE PROCEDURE [cqgdb].[SPOD]
    @idoption INT, 
    @datetime DATE,
    @price FLOAT(53),
    @impliedvol FLOAT(53),
    @timetoexpinyears FLOAT(53)
AS

SET NOCOUNT ON;

MERGE INTO cqgdb.tbloptiondata as tgt_tbloptiondata
USING
    (SELECT @idoption, @datetime)
    AS src_tbloptiondata (idoption, datetime)
    ON src_tbloptiondata.idoption = tgt_tbloptiondata.idoption
    AND src_tbloptiondata.datetime = tgt_tbloptiondata.datetime

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
    (@idoption, @datetime, @price, @impliedvol, @timetoexpinyears);

SET NOCOUNT OFF;