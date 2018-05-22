<?php
	session_start();
	require_once("./vendor/autoload.php");
	require_once("./db.php");
	
	date_default_timezone_set("America/New_York");
	
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
	
	function fdate($timestamp = false) {
		if($timestamp === false) {
			return "<a href='https://www.timeanddate.com/worldclock/fixedtime.html?msg=Time+Conversion&iso=" . date('Ymd\THis') . "&p1=179'>" . date('m/d/Y g:i A') . " EST</a>";
		}
		else {
			return "<a href='https://www.timeanddate.com/worldclock/fixedtime.html?msg=Time+Conversion&iso=" . date('Ymd\THis', $timestamp) . "&p1=179'>" . date('m/d/Y g:i A', $timestamp) . " EST</a>";
		}
	}
	
	include_once("./discord.php");
?>