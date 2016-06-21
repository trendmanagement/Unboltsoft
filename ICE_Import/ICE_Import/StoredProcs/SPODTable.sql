CREATE PROCEDURE [cqgdb].[SPODTable]
	@optiondata OptionDatasType READONLY
AS

CREATE TABLE tempOptionData
( 
	optionname varchar(50),
	idoption int,
	datetime date, 
	price float, 
	impliedvol float,
	timetoexpinyears float
)

INSERT INTO tempOptionData
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

UPDATE tempOptionData SET tempOptionData.idoption = tbloptions.idoption FROM tbloptions INNER JOIN tempOptionData ON
tempOptionData.optionname = tbloptions.optionname

SET NOCOUNT ON;

MERGE INTO cqgdb.tbloptiondata as tgt
USING tempOptionData AS src
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
DROP TABLE tempOptionData;
SET NOCOUNT OFF;
