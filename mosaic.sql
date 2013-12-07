SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='TRADITIONAL,ALLOW_INVALID_DATES';

CREATE SCHEMA IF NOT EXISTS `mosaic` DEFAULT CHARACTER SET utf8 COLLATE utf8_general_ci ;
USE `mosaic` ;

-- -----------------------------------------------------
-- Table `mosaic`.`Library`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `mosaic`.`Library` ;

CREATE TABLE IF NOT EXISTS `mosaic`.`Library` (
  `lid` INT NOT NULL AUTO_INCREMENT,
  `LibraryName` VARCHAR(128) NULL,
  `CreationDate` DATETIME NULL,
  PRIMARY KEY (`lid`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `mosaic`.`Image`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `mosaic`.`Image` ;

CREATE TABLE IF NOT EXISTS `mosaic`.`Image` (
  `iid` BIGINT NOT NULL AUTO_INCREMENT,
  `ImageSHA` VARCHAR(64) NULL,
  `ImagePath` VARCHAR(1024) NULL,
  `ImageR` FLOAT NULL,
  `ImageG` FLOAT NULL,
  `ImageB` FLOAT NULL,
  `ImageI` FLOAT NULL,
  `ImageCR` FLOAT NULL,
  `ImageCG` FLOAT NULL,
  `IMageCB` FLOAT NULL,
  `Img` LONGBLOB NULL,
  `ImageCnt` INT NULL,
  `ImageCreation` DATETIME NULL,
  `ImageLibrary` INT NULL,
  PRIMARY KEY (`iid`),
  INDEX `IMAGE_C_IDX` USING BTREE (`ImageR` ASC, `ImageG` ASC, `ImageB` ASC),
  INDEX `FK_IMAGE_LIBRARY_idx` (`ImageLibrary` ASC),
  INDEX `IMAGE_I_IDX` USING BTREE (`ImageI` ASC),
  UNIQUE INDEX `IMAGE_SHA_IDX` USING BTREE (`ImageSHA` ASC),
  CONSTRAINT `FK_IMAGE_LIBRARY`
    FOREIGN KEY (`ImageLibrary`)
    REFERENCES `mosaic`.`Library` (`lid`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB;

USE `mosaic` ;

-- -----------------------------------------------------
-- function ColorDistance
-- -----------------------------------------------------

USE `mosaic`;
DROP function IF EXISTS `mosaic`.`ColorDistance`;

DELIMITER $$
USE `mosaic`$$
CREATE FUNCTION `ColorDistance` (R1 FLOAT, G1 FLOAT, B1 FLOAT, LUM1 FLOAT, R2 FLOAT, G2 FLOAT, B2 FLOAT, LUM2 FLOAT)
RETURNS FLOAT
BEGIN
	DECLARE DR FLOAT;
	DECLARE DG FLOAT;
	DECLARE DB FLOAT;
	DECLARE DLUM FLOAT;

	SET DR = ABS(R1 - R2);
	SET DG = ABS(G1 - G2);
	SET DB = ABS(B1 - B2);
	SET DLUM = ABS(LUM1 - LUM2);	

	RETURN SQRT(DR*DR + DG*DG + DB*DB + DLUM*DLUM);
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure GetImages
-- -----------------------------------------------------

USE `mosaic`;
DROP procedure IF EXISTS `mosaic`.`GetImages`;

DELIMITER $$
USE `mosaic`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `GetImages`(IN R FLOAT, IN G FLOAT, IN B FLOAT, IN LUM FLOAT, IN DIST FLOAT, IN LID INT)
BEGIN
	DECLARE DTS FLOAT;
	SELECT i.ImageSHA, i.ImagePath, i.ImageCnt, i.Img, (DTS = ColorDistance(i.ImageR, i.ImageG, i.ImageB, i.ImageI, R, G, B, LUM)) AS ImageDistance  FROM mosaic.image i INNER JOIN mosaic.library l ON i.ImageLibrary = l.lid WHERE l.lid = LID AND DTS < DIST ORDER BY DTS ASC;
END$$

DELIMITER ;

SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;
