<?php
	require_once("./preload.php");
	
	function getround($tid) {
		global $db;
		$round = $db->query("SELECT MAX(round) FROM battles WHERE tid = " . $tid)->fetch();
		return (($round['MAX(round)'] > 0) ? $round['MAX(round)'] : "None");
	}
	
	function getstatus($tournament) {
		if(time() < $tournament['regstart']) {
			return "Planned";
		}
		elseif(time() < $tournament['regend']) {
			return "Registrations Open";
		}
		elseif(time() > $tournament['closure']) {
			return "Closed";
		}
		else {
			return "Round";
		}
	}
	
	function e4selectboxoptions($participantname, $currentlyselected) {
		$e4selectboxoptions = "";
		foreach($participantname AS $pid => $name) {
			$e4selectboxoptions .= "<option value='" . $pid . "' " . (($pid == $currentlyselected) ? "selected" : "") . ">" . $name . "</option>";
		}
		return $e4selectboxoptions;
	}
	
	if(!isset($admin)) {
		$pagetitle = "Access Denied";
		$pagecontent = "
			In order to access this page, you need to be logged into an administrator account.
		";
	}
	
	else {
		if(isset($_GET['action'])) {
			$action = $_GET['action'];
		}
		elseif(isset($_POST['action'])) {
			$action = $_POST['action'];
		}
		else {
			$action = false;
		}
		if(isset($_GET['tid']))  {
			$tid = $_GET['tid'];
		}
		elseif(isset($_POST['tid'])) {
			$tid = $_POST['tid'];
		}
		
		if($action == "domanagetournament") {
			if($_POST['subaction'] == "participant") { echo "yes";
				$commit = $db->prepare("UPDATE participants SET showdownusername = :showdownusername, team = :team, checked = :checked WHERE pid = :pid");
				$commit->execute(array(
					'showdownusername' => $_POST['showdownusername'],
					'team' => $_POST['team'],
					'checked' => $_POST['checked'],
					'pid' => $_POST['pid']
				));
				header('Location: ./controlpanel.php?action=managetournament&tid=' . $tid . '#pid' . $_POST['pid']);
			}
			elseif($_POST['subaction'] == "battle") {
				$commit = $db->prepare("UPDATE battles SET replay1 = :replay1, replay2 = :replay2, winner = :winner WHERE bid = :bid");
				$commit->execute(array(
					'replay1' => $_POST['replay1'],
					'replay2' => $_POST['replay2'],
					'winner' => $_POST['winner'],
					'bid' => $_POST['bid']
				));
				header('Location: ./controlpanel.php?action=managetournament&tid=' . $tid . '#bid' . $_POST['bid']);
			}
			else {
				$commit = $db->prepare("UPDATE tournaments SET tournament = :tournament, regstart = :regstart, regend = :regend, closure = :closure, roundend = :roundend, nextround = :nextround, minplayers = :minplayers, maxplayers = :maxplayers, e4member1 = :e4member1, e4member2 = :e4member2, e4member3 = :e4member3, e4member4 = :e4member4, e4member5 = :e4member5 WHERE tid = :tid");
				$commit->execute(array(
					'tournament' => $_POST['tournament'],
					'regstart' => strtotime($_POST['regstart']),
					'regend' => strtotime($_POST['regend']),
					'closure' => strtotime($_POST['closure']),
					'roundend' => strtotime($_POST['roundend']),
					'nextround' => strtotime($_POST['nextround']),
					'minplayers' => $_POST['minplayers'],
					'maxplayers' => $_POST['maxplayers'],
					'e4member1' => $_POST['e4member1'],
					'e4member2' => $_POST['e4member2'],
					'e4member3' => $_POST['e4member3'],
					'e4member4' => $_POST['e4member4'],
					'e4member5' => $_POST['e4member5'],
					'tid' => $tid
				));
				header('Location: ./controlpanel.php?action=managetournament&tid=' . $tid);
			}
		}
		elseif($action == "donewtournament") {
			$create = $db->prepare("INSERT INTO tournaments (tournament, regstart, regend, closure, roundend, nextround, minplayers, maxplayers) VALUES (:tournament, :regstart, :regend, :closure, :roundend, :nextround, :minplayers, :maxplayers)");
			$create-> execute(array(
				'tournament' => $_POST['tournament'],
				'regstart' => strtotime($_POST['regstart']),
				'regend' => strtotime($_POST['regend']),
				'closure' => strtotime($_POST['closure']),
				'roundend' => strtotime($_POST['roundend']),
				'nextround' => strtotime($_POST['nextround']),
				'minplayers' => $_POST['minplayers'],
				'maxplayers' => $_POST['maxplayers']
			));
			header('Location: ./controlpanel.php?action=managetournament&tid=' . $db->lastInsertId());
		}
		elseif($action == "managetournament") {
			$pagesubtitle = "Tournament Management [#" . $tid . "]";
			$round = 0;
			$participantlist = "";
			$participantmenu = "";
			$participantname = array();
			$battlelist = "";
			$battlemenu = "";
			
			$tournament = $db->query("SELECT * FROM tournaments WHERE tid = " . $tid)->fetch();
			$participant_q = $db->query("SELECT * FROM participants WHERE tid = " . $tid . " ORDER BY pid ASC");
			while($participant = $participant_q->fetch()) {
				$participantlist .= "
					<form method='post' action='controlpanel.php' id='pid" . $participant['pid'] . "'>
						Discord Username / UID: <input type='text' value=\"" . $participant['discordusername'] . "\" readonly> <@" . $participant['uid'] . "><br>
						<label for='showdownusername'>Showdown Username:</label> <input type='text' name='showdownusername' id='showdownusername' value=\"" . $participant['showdownusername'] . "\"><br>
						<label for='team'>Showdown Team:</label> <textarea name='team' id='team'>" . $participant['team'] . "</textarea><br>
						<label for='checked'>Team Checked:</label> <input type='checkbox' name='checked' id='checked' value='1' " . ($participant['checked'] ? "checked" : "") . "><br>
						<input type='submit' value='Commit Changes'>
						<input type='hidden' name='action' value='domanagetournament'>
						<input type='hidden' name='subaction' value='participant'>
						<input type='hidden' name='tid' value='" . $tid . "'>
						<input type='hidden' name='pid' value='" . $participant['pid'] . "'>
					</form><br>
					<br>
				";
				$participantname[$participant['pid']] = $participant['discordusername'];
				$participantmenu .= "<a href='#pid" . $participant['pid'] . "'>" . $participantname[$participant['pid']] . "</a><br>";
			}
			
			$battle_q = $db->query("SELECT * FROM battles WHERE tid = " . $tid . " ORDER BY round ASC, bid ASC");
			while($battle = $battle_q->fetch()) {
				if($battle['round'] > $round) {
					$round = $battle['round'];
					$battlelist .= "<em>Round " . $round . "</em><br><br>";
				}
				$battlelist .= "
					<form method='post' action='controlpanel.php' id='bid" . $battle['bid'] . "'>
						Player 1: <input type='text' value=\"" . $participantname[$battle['player1']] . "\" readonly><br>
						Player 2: <input type='text' value=\"" . ($battle['player2'] ? $participantname[$battle['player2']] : "Bye") . "\" readonly><br>
						<label for='replay1'>Player 1's Replay:</label> <input type='text' name='replay1' id='replay1' value=\"" . $battle['replay1'] . "\"><br>
						<label for='replay2'>Player 2's Replay:</label> <input type='text' name='replay2' id='replay2' value=\"" . $battle['replay2'] . "\"><br>
						<label for='winner'>Winner:</label> <select name='winner' id='winner'>
							<option value='0'>None</option>
							<option value='1' " . ($battle['winner'] == 1 ? "selected" : '') . ">Player 1</option>
							<option value='2' " . ($battle['winner'] == 2 ? "selected" : '') . ">Player 2</option>
						</select><br>
						<input type='submit' value='Commit Changes'>
						<input type='hidden' name='action' value='domanagetournament'>
						<input type='hidden' name='subaction' value='battle'>
						<input type='hidden' name='tid' value='" . $tid . "'>
						<input type='hidden' name='bid' value='" . $battle['bid'] . "'>
					</form><br>
					<br>
				";
				$battlemenu .= "<a href='#bid" . $participant['bid'] . "'>Round " . $round . " - " . $participantname[$battle['player1']] . " VS " . ($battle['player2'] ? $participantname[$battle['player2']] : "Bye") . "</a><br>";
			}
			
			$pagecontent = "
				<div class='inpagemenu'>
					<a href='#tournament'>Tournament Details</a><br>
					<a href='#participant'>Participants</a><br>
					<a href='#battle'>Battles</a><br>
				</div><br>
				
				<strong id='tournament'>Tournament Details</strong><br>
				<br>
				<form method='post' action='controlpanel.php'>
					<label for='tournament'>Tournament Name:</label> <input type='text' name='tournament' id='tournament' value=\"" . $tournament['tournament'] . "\"><br>
					<br>
					<label for='regstart'>Registrations Opening Date:</label> <input type='text' name='regstart' id='regstart' value='" . date('m/d/Y g:i A', $tournament['regstart']) . "'><br>
					<label for='regend'>Registrations Closure Date:</label> <input type='text' name='regend' id='regend' value='" . date('m/d/Y g:i A', $tournament['regend']) . "'><br>
					<label for='closure'>Tournament Closure Date:</label> <input type='text' name='closure' id='closure' value='" . date('m/d/Y g:i A', $tournament['closure']) . "'><br>
					<br>
					<label for='minplayers'>Minimum # of Players:</label> <input type='text' name='minplayers' id='minplayers' value=\"" . $tournament['minplayers'] . "\"><br>
					<label for='maxplayers'>Maximum # of Players:</label> <input type='text' name='maxplayers' id='maxplayers' value=\"" . $tournament['maxplayers'] . "\"><br>
					<br>
					Current Round: " . getround($tid) . "<br>
					<label for='roundend'>Current Round Ends:</label> <input type='text' name='roundend' id='roundend' value='" . date('m/d/Y g:i A', $tournament['roundend']) . "'><br>
					<label for='nextround'>Next Round Starts:</label> <input type='text' name='nextround' id='nextround' value='" . date('m/d/Y g:i A', $tournament['nextround']) . "'><br>
					<br>
					<label for='e4member1'>Elite 4 Member 1:</label> <select name='e4member1' id='e4member1'><option value='0'>None Selected</option>" . e4selectboxoptions($participantname, $tournament['e4member1']) . "</select><br>
					<label for='e4member2'>Elite 4 Member 2:</label> <select name='e4member2' id='e4member2'><option value='0'>None Selected</option>" . e4selectboxoptions($participantname, $tournament['e4member2']) . "</select><br>
					<label for='e4member3'>Elite 4 Member 3:</label> <select name='e4member3' id='e4member3'><option value='0'>None Selected</option>" . e4selectboxoptions($participantname, $tournament['e4member3']) . "</select><br>
					<label for='e4member4'>Elite 4 Member 4:</label> <select name='e4member4' id='e4member4'><option value='0'>None Selected</option>" . e4selectboxoptions($participantname, $tournament['e4member4']) . "</select><br>
					<label for='e4member5'>Elite 4 Champion:</label> <select name='e4member5' id='e4member5'><option value='0'>None Selected</option>" . e4selectboxoptions($participantname, $tournament['e4member5']) . "</select><br>
					<br>
					<input type='submit' value='Commit Changes'>
					<input type='hidden' name='action' value='domanagetournament'>
					<input type='hidden' name='subaction' value='tournament'>
					<input type='hidden' name='tid' value='" . $tid . "'>
				</form><br>
				<br>
				<strong id='participant'>Participants</strong><br>
				<br>
				<div class='inpagemenu'>
					" . $participantmenu . "
				</div><br>
				" . $participantlist . "<br>
				<br>
				<strong id='battle'>Battles</strong><br>
				<br>
				<div class='inpagemenu'>
					" . $battlemenu . "
				</div><br>
				" . $battlelist . "
			";
		}
		elseif($action == "newtournament") {
			$pagesubtitle = "New Tournament";
			
			$pagecontent .= "
				<form method='post' action='controlpanel.php'>
					<label for='tournament'>Tournament Name:</label> <input type='text' name='tournament' id='tournament'><br>
					<br>
					<label for='regstart'>Registrations Opening Date:</label> <input type='text' name='regstart' id='regstart' placeholder='" . date('m/d/Y g:i A') . "'><br>
					<label for='regend'>Registrations Closure Date:</label> <input type='text' name='regend' id='regend' placeholder='" . date('m/d/Y g:i A') . "'><br>
					<label for='closure'>Tournament Closure Date:</label> <input type='text' name='closure' id='closure' placeholder='" . date('m/d/Y g:i A') . "'><br>
					<br>
					<label for='minplayers'>Minimum # of Players:</label> <input type='text' name='minplayers' id='minplayers'><br>
					<label for='maxplayers'>Maximum # of Players:</label> <input type='text' name='maxplayers' id='maxplayers'><br>
					<br>
					<label for='roundend'>End of Round 1:</label> <input type='text' name='roundend' id='roundend' placeholder='" . date('m/d/Y g:i A') . "'><br>
					<label for='nextround'>Beginning of Round 2:</label> <input type='text' name='nextround' id='nextround' placeholder='" . date('m/d/Y g:i A') . "'><br>
					<br>
					<input type='submit' value='Create Tournament'>
					<input type='hidden' name='action' value='donewtournament'>
				</form>
			";
		}
		else {
			$pagesubtitle = "Tournament List";
			
			$tournamentlist = "
				<strong><a href='./controlpanel.php?action=newtournament'>New Tournament</a></strong><br><br>
			";
			$tournamentsublist = array(
				"Planned" => false,
				"Registrations Open" => false,
				"Closed" => false,
				"Round" => false
			);
			
			$tournament_q = $db->query("SELECT * FROM tournaments ORDER BY regstart DESC");
			while($tournament = $tournament_q->fetch()) {
				$tournament['status'] = getstatus($tournament);
				if($tournament['status'] == "Round") {
					$tournament['round'] =  " " . getround($tournament['tid']);
				}
				else {
					$tournament['round'] = "";
				}
				$tournamentsublist[$tournament['status']] .= "
					<a href='./controlpanel.php?action=managetournament&tid=" . $tournament['tid'] . "'>Tournament #" . $tournament['tid'] . " : " . $tournament['tournament'] . "</a><br>
					Status: " . $tournament['status'] . $tournament['round'] . "<br><br>
				";
			}
			
			$tournamentlist .= "
				" . ($tournamentsublist['Round'] ? "<strong>Running Tournaments</strong><br><br> " . $tournamentsublist['Round'] : "") . "
				" . ($tournamentsublist['Registrations Open'] ? "<strong>Tournaments Open for Registration</strong><br><br> " . $tournamentsublist['Registrations Open'] : "") . "
				" . ($tournamentsublist['Planned'] ? "<strong>Planned Tournaments</strong><br><br> " . $tournamentsublist['Planned'] : "") . "
				" . ($tournamentsublist['Closed'] ? "<strong>Past Tournaments</strong><br><br> " . $tournamentsublist['Closed'] : "") . "
			";
			
			$pagecontent = $tournamentlist;
		}
		
		$pagetitle = "Control Panel - " . $pagesubtitle;
	}
	
	include("./global.php");
?>