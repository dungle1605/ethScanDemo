-- phpMyAdmin SQL Dump
-- version 5.2.0
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: Mar 05, 2023 at 03:23 PM
-- Server version: 10.4.27-MariaDB
-- PHP Version: 8.0.25

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `ethscan`
--

DELIMITER $$
--
-- Procedures
--
CREATE DEFINER=`root`@`localhost` PROCEDURE `SPP_INSERT_BLOCK` (IN `in_blockNumber` INT, IN `in_hash` VARCHAR(66), IN `in_parentHash` VARCHAR(66), IN `in_miner` VARCHAR(42), IN `in_blockReward` DECIMAL(50,0), IN `in_gasLimit` DECIMAL(50,0), IN `in_gasUsed` DECIMAL(50,0))   BEGIN
	DECLARE isExistedBlockNo BIT DEFAULT 0;
    
    SELECT 1
    INTO isExistedBlockNo
    FROM blocks
    WHERE blockNumber = `in_blockNumber`;
    
    IF(isExistedBlockNo = 0) THEN
    	INSERT INTO blocks(blockNumber, `hash`, parentHash, miner, blockReward, gasLimit, gasUsed)
		VALUES(`in_blockNumber`, `in_hash`, `in_parentHash`, `in_miner`, `in_blockReward`, `in_gasLimit`, `in_gasUsed`);
    END IF;
	
END$$

CREATE DEFINER=`root`@`localhost` PROCEDURE `SPP_INSERT_TRANSACTION` (IN `in_blockNumber` INT, IN `in_hash` VARCHAR(66), IN `in_from` VARCHAR(42), IN `in_to` VARCHAR(42), IN `in_value` DECIMAL(50,0), IN `in_gas` DECIMAL(50,0), IN `in_gasPrice` DECIMAL(50,0), IN `in_transactionIndex` INT)   BEGIN
	DECLARE temp_blockID INT;
    
    SELECT blockID INTO temp_blockID FROM blocks
    WHERE blockNumber = in_blockNumber
    LIMIT 1;
    
    INSERT INTO transactions(blockID, `hash`, `from`, `to`, `value`, gas, gasPrice, transactionIndex)
    VALUES(temp_blockID, in_hash, in_from, in_to, in_value, in_gas, in_gasPrice, in_transactionIndex);

END$$

DELIMITER ;

-- --------------------------------------------------------

--
-- Table structure for table `blocks`
--

CREATE TABLE `blocks` (
  `blockID` int(20) NOT NULL,
  `blockNumber` int(20) DEFAULT NULL,
  `hash` varchar(66) DEFAULT NULL,
  `parentHash` varchar(66) DEFAULT NULL,
  `miner` varchar(42) DEFAULT NULL,
  `blockReward` decimal(50,0) DEFAULT NULL,
  `gasLimit` decimal(50,0) DEFAULT NULL,
  `gasUsed` decimal(50,0) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `transactions`
--

CREATE TABLE `transactions` (
  `transactionID` int(20) NOT NULL,
  `blockID` int(20) NOT NULL,
  `hash` varchar(66) DEFAULT NULL,
  `from` varchar(42) DEFAULT NULL,
  `to` varchar(42) DEFAULT NULL,
  `value` decimal(50,0) DEFAULT NULL,
  `gas` decimal(50,0) DEFAULT NULL,
  `gasPrice` decimal(50,0) DEFAULT NULL,
  `transactionIndex` int(20) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `blocks`
--
ALTER TABLE `blocks`
  ADD PRIMARY KEY (`blockID`);

--
-- Indexes for table `transactions`
--
ALTER TABLE `transactions`
  ADD PRIMARY KEY (`transactionID`),
  ADD KEY `blockID` (`blockID`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `blocks`
--
ALTER TABLE `blocks`
  MODIFY `blockID` int(20) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=19;

--
-- AUTO_INCREMENT for table `transactions`
--
ALTER TABLE `transactions`
  MODIFY `transactionID` int(20) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=384;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `transactions`
--
ALTER TABLE `transactions`
  ADD CONSTRAINT `transactions_ibfk_1` FOREIGN KEY (`blockID`) REFERENCES `blocks` (`blockID`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
