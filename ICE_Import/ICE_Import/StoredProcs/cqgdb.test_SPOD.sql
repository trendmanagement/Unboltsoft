CREATE PROCEDURE [cqgdb].[test_SPOD]
	@optionmonth CHAR (1),
	@optionyear INT, 
	@datetime DATE,
	@price FLOAT(53),
	@impliedvol FLOAT(53),
	@timetoexpinyears FLOAT(53)
AS

SET NOCOUNT ON;

MERGE INTO cqgdb.test_tbloptiondata as tgt_tbloptiondata
USING
	(SELECT * FROM [cqgdb].test_tbloptions WHERE optionmonth = @optionmonth AND optionyear = @optionyear)
	AS src_tbloptiondata
	ON tgt_tbloptiondata.idoption = src_tbloptiondata.idoption
	AND tgt_tbloptiondata.datetime = @datetime
	and tgt_tbloptiondata.timetoexpinyears = @timetoexpinyears

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
	(src_tbloptiondata.idoption, @datetime, @price, @impliedvol, @timetoexpinyears);

SET NOCOUNT OFF;