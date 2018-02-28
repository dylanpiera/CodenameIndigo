<?php
	session_start();
	require_once("./vendor/autoload.php");
	require_once("./db.php");
	
	if(isset($_SESSION['user'])) {
		$uid = $_SESSION['user']['id'];
		$username = $_SESSION['user']['username'];
		$discriminator = $_SESSION['user']['discriminator'];
		$fullname = $username . "#" . $discriminator;
		$avatar = "https://cdn.discordapp.com/avatars/" . $uid . "/" . $_SESSION['user']['avatar'] . ".png";
		
		if($checkadmin = $db->query("SELECT * FROM admins WHERE uid = " . $uid)->fetch()) {
			$admin = true;
		}
	}
	
	function esc($s) {
		return str_replace('"', "&quot;", $s);
	}
	
	function dbesc($s) {
		return str_replace("'", "''", $s);
	}
	
	include_once("./discord.php");
?>