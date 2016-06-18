CREATE PROCEDURE [cqgdb].[SPODTable]
	@optiondata OptionDatasType READONLY
AS

CREATE TABLE temp
( 
	optionname varchar(50),
	idoption int,
	datetime date, 
	price float, 
	impliedvol float,
	timetoexpinyears float
)

INSERT INTO temp
(
	datetime, 
	price, 
	impliedvol,
	timetoexpinyears
)
SELECT
	datetime, 
	price, 
	impliedvol,
	timetoexpinyears
FROM @optiondata

UPDATE temp SET temp.idoption = tbloptions.idoption FROM  tbloptions INNER JOIN temp ON
temp.optionname = tbloptions.optionname

SET NOCOUNT ON;

MERGE INTO cqgdb.tbloptiondata as tgt
USING temp AS src
    ON tgt.idoption = src.idoption
    AND tgt.datetime = src.datetime
	AND tgt.price = src.price

WHEN MATCHED THEN
UPDATE
    SET 
    price =src.price,
    impliedvol =src.impliedvol,
    timetoexpinyears = src.timetoexpinyears

WHEN NOT MATCHED THEN

    INSERT 
    (idoption,
	 datetime, 
	 price, 
	 impliedvol,
	 timetoexpinyears)
    VALUES 
    (src.idoption, 
	src.datetime, 
	src.price, 
	src.impliedvol, 
	src.timetoexpinyears);
DROP TABLE temp;
SET NOCOUNT OFF;
