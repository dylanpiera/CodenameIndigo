<?php
	session_start();
	require_once("./vendor/autoload.php");
	require_once("./db.php");
	require_once("BLBot/run.php");
	
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
		elseif($checkskinedit = $db->query("SELECT * FROM `skineditor` WHERE uid = " . $uid)->fetch()) {
			$skinedit = true;
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

	function getColorFromDB(string $col, $db, $uid) {
    try {
        if($col_db = $db->query("SELECT * FROM `colors` WHERE `uid` = '".dbesc($uid)."'")->fetch()) {
            $color = $col_db[$col];
        }
        elseif($col_db = $db->query("SELECT * FROM `colors` WHERE `uid` = '0'")->fetch()) {
            $color = $col_db[$col];
        }
        else {
            $color = "#fff";
        }
    }
    catch (Exception $e) {
        return "#fff";
    }
    return $color;
}
	
	include_once("./discord.php");
?>