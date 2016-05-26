CREATE PROCEDURE [cqgdb].[test_SPO_Mod]
	@optionname VARCHAR (45), 
	@optionmonth CHAR (1), 
	@optionmonthint INT, 
	@optionyear INT, 
	@strikeprice FLOAT (53), 
	@callorput CHAR (1), 
	@idinstrument BIGINT, 
	@cqgsymbol VARCHAR (45)
AS

MERGE INTO cqgdb.test_tbloptions as tgttest_tbloptions
USING
	(SELECT * FROM [cqgdb].test_tblcontracts WHERE monthint = @optionmonthint  AND year = @optionyear)
	AS src_test_tblcontract

	ON tgttest_tbloptions.idcontract = src_test_tblcontract.idcontract
	AND tgttest_tbloptions.optionmonthint = @optionmonthint
	AND tgttest_tbloptions.optionyear = @optionyear
	AND tgttest_tbloptions.strikeprice = @strikeprice
	AND tgttest_tbloptions.callorput = @callorput
	AND tgttest_tbloptions.idinstrument = @idinstrument 
	AND tgttest_tbloptions.expirationdate = src_test_tblcontract.expirationdate

WHEN MATCHED THEN
UPDATE
	SET 
	optionname = @optionname,
	cqgsymbol = @cqgsymbol,
	expirationdate = src_test_tblcontract.expirationdate

WHEN NOT MATCHED THEN
	INSERT 	
	(optionname,
	optionmonth,
	optionmonthint,
	optionyear,
	strikeprice,
	callorput,
	idinstrument,
	expirationdate,
	idcontract,
	cqgsymbol)
	VALUES 
	(@optionname, 
	@optionmonth, 
	@optionmonthint,
	@optionyear, 
	@strikeprice, 
	@callorput,
	@idinstrument, 
	src_test_tblcontract.expirationdate,
	src_test_tblcontract.idcontract,
	@cqgsymbol);
SET NOCOUNT ON;