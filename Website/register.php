<?php
	require_once("./preload.php");
	
	if(!isset($uid)) {
		$pagetitle = "Access Denied";
		$pagecontent = "
			In order to access this page, you need to be logged in.
		";
	}
	
	else {
		$notice = "";
		if(isset($_GET['action'])) {
			if($_GET['action'] == "unregister") {
				if($tournament = $db->query("SELECT * FROM tournaments WHERE tid = '" . dbesc($_GET['tid']) . "'")->fetch()) {
					if(time() >= $tournament['regstart'] AND time() <= $tournament['regend']) {
						$db->query("DELETE FROM participants WHERE tid = '" . dbesc($_GET['tid']) . "' AND uid = " . $uid);
						$notice = "<div class='notice green'>Your registration has been cancelled..</div>";
					}
					else {
						$notice = "<div class='notice red'>Registrations for this tournament are not open.</div>";
					}
				}
				else {
					$notice = "<div class='notice red'>The requested tournament does not exist.</div>";
				}
			}
		}
		if(isset($_POST['tid'])) {
			if($tournament = $db->query("SELECT * FROM tournaments WHERE tid = '" . dbesc($_POST['tid']) . "'")->fetch()) {
				$registration = $db->query("SELECT * FROM participants WHERE tid = " . $tournament['tid'] . " AND uid = " . $uid)->fetch();
				if(time() >= $tournament['regstart'] AND time() <= $tournament['regend']) {
					if($registration) {
						$db->query("UPDATE participants SET discordusername = '" . dbesc($fullname) . "', showdownusername = '" . dbesc($_POST['showdown']) . "', team = '" . dbesc($_POST['team']) . "' WHERE tid = '" . dbesc($_POST['tid']) . "' AND uid = " . $uid);
						$notice = "<div class='notice green'>Your registration details have been updated.</div>";
					}
					else {
						$db->query("INSERT INTO participants(tid, uid, discordusername, showdownusername, team) VALUES('" . dbesc($_POST['tid']) . "', " . $uid . ", '" . dbesc($fullname) . "', '" . dbesc($_POST['showdown']) . "', '" . dbesc($_POST['team']) . "')");
						$notice = "<div class='notice green'>Your registration details have been saved.</div>";
					}
				}
				else {
					$notice = "<div class='notice red'>Registrations for this tournament are not open.</div>";
				}
			}
			else {
				$notice = "<div class='notice red'>The requested tournament does not exist.</div>";
			}
		}
		
		$tournamentlist = "";
		$tournament_q = $db->query("SELECT * FROM tournaments WHERE closure >= " . time() . " ORDER BY regstart DESC");
		while($tournament = $tournament_q->fetch()) {
			if(time() < $tournament['regstart']) {
				$tournamentlist .= "
					<div class='tournament'>
						<strong>" . $tournament['tournament'] . "</strong><br>
						<em>Registrations for this tournament have not started yet.</em><br>
						<span class='smalltext'>Registrations open: <a href='https://www.timeanddate.com/worldclock/fixedtime.html?msg=Time+Conversion&iso=" . date('Ymd\THis', $tournament['regstart']) . "&p1=195'>" . date('d/m/Y H:i', $tournament['regstart']) . " CET</a><br>
						Registrations close: <a href='https://www.timeanddate.com/worldclock/fixedtime.html?msg=Time+Conversion&iso=" . date('Ymd\THis', $tournament['regend']) . "&p1=195'>" . date('d/m/Y H:i', $tournament['regend']) . " CET</a></span>
					</div>
				";
			}
			else {
				$registration = $db->query("SELECT * FROM participants WHERE tid = " . $tournament['tid'] . " AND uid = " . $uid)->fetch();
				$registered = $db->query("SELECT COUNT(pid) FROM participants WHERE tid = " . $tournament['tid'])->fetch();
				if($registration) {
					$regposition = $db->query("SELECT COUNT(pid) + 1 FROM participants WHERE tid = " . $tournament['tid'] . " AND pid < " . $registration['pid'])->fetch();
					if($regposition['COUNT(pid) + 1'] > $tournament['maxplayers']) {
						$regmsg = "You are in the waiting list for this tournament.";
					}
					else {
						$regmsg = "You are registered for this tournament.";
					}
				}
				else {
					$regmsg = "You are not registered for this tournament.";
				}
				if(time() <= $tournament['regend']) {
					$tournamentlist .= "
						<div class='tournament'>
							<strong>" . $tournament['tournament'] . "</strong><br>
							<em>" . $regmsg . "<br>
							Registrations for this tournament are currently open, fill the form below to " . ($registration ? "edit your registration details" : "register") . ".</em><br>
							<form method='post' action='register.php'>
								<table>
									<tr>
										<td><label for='discord'>Discord Username:</label></td>
										<td><input type='text' name='discord' id='discord' value=\"" . esc($fullname) . "\" readonly></td>
									</tr>
									<tr>
										<td><label for='showdown'>Showdown Username:</label></td>
										<td><input type='text' name='showdown' id='showdown' value=\"" . esc($registration['showdownusername']) . "\" maxlength='18'></td>
									</tr>
									<tr>
										<td><label for='team'>Team:</label></td>
										<td><textarea name='team' id='team' cols='22' rows='1'>" . $registration['team'] . "</textarea></td>
									</tr>
									<tr>
										<td colspan='2'>
											<input type='hidden' name='tid' value='" . $tournament['tid'] . "'>
											<input type='submit' value='" . ($registration ? "Edit Registration Details" : "Register") . "'>
										</td>
									</tr>
								</table>
							</form>
							<span class='smalltext'>Participants registered: " . $registered['COUNT(pid)'] . "/" . $tournament['maxplayers'] . "<br>
							Registrations close: <a href='https://www.timeanddate.com/worldclock/fixedtime.html?msg=Time+Conversion&iso=" . date('Ymd\THis', $tournament['regend']) . "&p1=195'>" . date('d/m/Y H:i', $tournament['regend']) . " CET</a>
							" . ($registration ? "<br><a href='register.php?action=unregister&tid=" . $tournament['tid'] . "'>Click here to cancel your registration.</a>" : "") . "</span>
						</div>
					";
				}
				else {
					if($registration) {
						$tournamentlist .= "
							<div class='tournament'>
								<strong>" . $tournament['tournament'] . "</strong><br>
								<em>" . $regmsg . "<br>
								Registrations for this tournament are closed, you can see your registration details below, but not edit them readonly.</em><br>
								<table>
									<tr>
										<td><label for='discord'>Discord Username:</label></td>
										<td><input type='text' name='discord' id='discord' value=\"" . esc($fullname) . "\" readonly></td>
									</tr>
									<tr>
										<td><label for='showdown'>Showdown Username:</label></td>
										<td><input type='text' name='showdown' id='showdown' value=\"" . esc($registration['showdownusername']) . "\" maxlength='18' readonly></td>
									</tr>
									<tr>
										<td><label for='team'>Team:</label></td>
										<td><textarea name='team' id='team' cols='22' rows='1' readonly>" . $registration['team'] . "</textarea></td>
									</tr>
								</table>
								<span class='smalltext'>Participants registered: " . $registered['COUNT(pid)'] . "/" . $tournament['maxplayers'] . "<br>
								Registrations close: <a href='https://www.timeanddate.com/worldclock/fixedtime.html?msg=Time+Conversion&iso=" . date('Ymd\THis', $tournament['regend']) . "&p1=195'>" . date('d/m/Y H:i', $tournament['regend']) . " CET</a></span>
							</div>
						";
					}
					else {
						$tournamentlist .= "
							<div class='tournament'>
								<strong>" . $tournament['tournament'] . "</strong><br>
								<em>Registrations for this tournament are closed and you have not registered for it.</em>
							</div>
						";
					}
				}
			}
		}
		if(!isset($tournamentlist)) {
			$tournamentlist = "There is no tournament running nor planned at the moment.";
		}
		
		$pagetitle = "Registration";
		$pagecontent = "
			" . $notice . "
			" . $tournamentlist . "
		";
	}
	
	include("./global.php");
?>