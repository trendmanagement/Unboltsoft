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

MERGE INTO cqgdb.test_tbloptions as tgt_tbloptions
USING
	(SELECT * FROM test_tbloptions
	WHERE test_tbloptions.idcontract = (SELECT test_tblcontracts.idcontract FROM [cqgdb].test_tblcontracts WHERE month = @optionmonth AND year = @optionyear))
	AS src_tbl
	ON  src_tbl.optionmonthint = @optionmonthint
	AND src_tbl.optionyear = @optionyear
	AND src_tbl.strikeprice = @strikeprice
	AND src_tbl.callorput = @callorput
	AND src_tbl.idinstrument = @idinstrument 

WHEN MATCHED THEN
UPDATE
	SET 
	optionname = @optionname,
	cqgsymbol = @cqgsymbol,
	expirationdate = src_tbl.expirationdate

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
	src_tbl.expirationdate,
	src_tbl.idcontract,
	@cqgsymbol);
SET NOCOUNT ON;