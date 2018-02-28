<?php
	try {
		$db = new PDO("mysql:host=localhost;dbname=bulbaleague;charset=utf8mb4", "saphir", "ANothER1993", array(PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION));
	}
	catch(Exception $e) {
		die("Error: " . $e->getMessage());
	}
?>