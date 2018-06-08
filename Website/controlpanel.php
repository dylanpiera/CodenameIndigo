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
	
	if(!isset($admin) && !isset($skinedit)) {
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
			<div class='row'>
				<div class='col-6 col-sm-6 col-md-6 col-lg-6 col-xl-6'>
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
				</div>
			</div>
			";
		}
		elseif($action == "newtournament") {
			$pagesubtitle = "New Tournament";
			
			$pagecontent .= "
			<div class='row'>
				<div class='col-6 col-sm-6 col-md-6 col-lg-6 col-xl-6'>
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
				</div>
			</div>
			";
		}
		elseif($action == "changecolors") {

			$pagetitle = "Saving...";
			$pagecontent = "<a href='https://bulbaleague.soaringnetwork.com/controlpanel.php'>Click here if the page doesn't load...</a>";
			
			if(isset($_POST['overrideDefault']))
			{
				if($_POST['overrideDefault'] == 'checked') {
					$db->query("DELETE FROM `colors` WHERE `uid` = 0")->execute();
					$commit = $db->prepare("INSERT INTO `colors` (`uid`, `mbgcolor`, `hbgcolor`, `smbgcolor`, `ssbgcolor`, `mccolor`, `sbcolor`, `linkcolor`) VALUES (0, :mbgcolor, :hbgcolor, :smbgcolor, :ssbgcolor, :mccolor, :sbcolor, :linkcolor)");
					$commit->execute(array(
						"mbgcolor" => $_POST['mbgcolor'],
						"hbgcolor" => $_POST['hbgcolor'],
						"smbgcolor" => $_POST['smbgcolor'],
						"ssbgcolor" => $_POST['ssbgcolor'],
						"mccolor" => $_POST['mccolor'],
						"sbcolor" => $_POST['sbcolor'],
						"linkcolor" => $_POST['linkcolor']
					));
				}
			}
			if(isset($_POST['overrideUserData']))
			{
				if($_POST['overrideUserData'] == 'checked') {
					$db->query("DELETE FROM `colors` WHERE `uid` != 0")->execute();
				}
			}
			else {
				$db->query("DELETE FROM `colors` WHERE `uid` = '".dbesc($uid)."'")->execute();
				$commit = $db->prepare("INSERT INTO `colors` (`uid`, `mbgcolor`, `hbgcolor`, `smbgcolor`, `ssbgcolor`, `mccolor`, `sbcolor`, `linkcolor`) VALUES (".dbesc($uid).", :mbgcolor, :hbgcolor, :smbgcolor, :ssbgcolor, :mccolor, :sbcolor, :linkcolor)");
				$commit->execute(array(
					"mbgcolor" => $_POST['mbgcolor'],
					"hbgcolor" => $_POST['hbgcolor'],
					"smbgcolor" => $_POST['smbgcolor'],
					"ssbgcolor" => $_POST['ssbgcolor'],
					"mccolor" => $_POST['mccolor'],
					"sbcolor" => $_POST['sbcolor'],
					"linkcolor" => $_POST['linkcolor']
				));
			}

			header("Location: ./controlpanel.php");
		}
		else {
			$pagesubtitle = "Tournament List";
			
			$tournamentlist = "
				<div class='row'>
					<div class='col-6 col-sm-6 col-md-6 col-lg-6 col-xl-6'>
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
			
			$colors = array(
				"mbgcolor" => getColorFromDB("mbgcolor", $db, $uid),
				"hbgcolor" => getColorFromDB("hbgcolor", $db, $uid),
				"smbgcolor" => getColorFromDB("smbgcolor", $db, $uid),
				"ssbgcolor" => getColorFromDB("ssbgcolor", $db, $uid),
				"mccolor" => getColorFromDB("mccolor", $db, $uid),
				"sbcolor" => getColorFromDB("sbcolor", $db, $uid),
				"linkcolor" => getColorFromDB("linkcolor", $db, $uid)
			);

			if(!isset($admin))
			{
				$isDisabled = "disabled";
			}
			$skincoloredits = "
				</div>
				<div class='col-12 col-sm-12 col-md-6 col-lg-6 col-xl-6' style='background-color: #ffffffb0; '>
				<style>.form-row {padding-bottom:10px;}</style>
					<strong>Color Settings:</strong><br><br>

					<form method='post' action='controlpanel.php'>
						<div class='row form-row'>
						<!--form method='post' action='controlpanel.php'-->
						<div class='col-md-8 mb-6'>
							<label for='mbgcolor'>Main Background Color:</label><br>
								<div class='input-group'> 
									<div class='input-group-prepend'>
										<span class='input-group-text' id='igp1' style='background-color: ".$colors['mbgcolor']."; border-color: ".$colors['mbgcolor']."85;'>@</span>
									</div>
									<input type='text' class='form-control' aria-describedby='igp1' placeholder='".$colors['mbgcolor']."' value='".$colors['mbgcolor']."' name='mbgcolor' id='mbgcolor' maxlength='7' required>
								</div>
						</div>
						<div class='col-md-4 mb-6' style='background-color: ".$colors['mbgcolor']."; border-color: ".$colors['mbgcolor']."85;'><span></span></div>
						</div>
						<div class='row form-row'>
						<div class='col-md-8 mb-6'>
							<label for='hbgcolor'>Banner Background Color:</label> 
							<div class='input-group'> 
								<div class='input-group-prepend'>
									<span class='input-group-text' id='igp2' style='background-color: ".$colors['hbgcolor']."; border-color: ".$colors['hbgcolor']."85;'>@</span>
								</div>
								<input type='text' class='form-control' aria-describedby='igp2' placeholder='".$colors['hbgcolor']."' value='".$colors['hbgcolor']."' name='hbgcolor' id='hbgcolor' maxlength='7' required>
							</div>
						</div>
						<div class='col-md-4 mb-6' style='background-color: ".$colors['hbgcolor']."; border-color: ".$colors['hbgcolor']."85;'><span></span></div>
						</div>
						<div class='row form-row'>
						<div class='col-md-8 mb-6'>
							<label for='smbgcolor'>Sidebar Background Color:</label> 
							<div class='input-group'> 
								<div class='input-group-prepend'>
									<span class='input-group-text' id='igp3' style='background-color: ".$colors['smbgcolor']."; border-color: ".$colors['smbgcolor']."85;'>@</span>
								</div>
								<input type='text' class='form-control' aria-describedby='igp3' placeholder='".$colors['smbgcolor']."' value='".$colors['smbgcolor']."' name='smbgcolor' id='smbgcolor' maxlength='7' required>
							</div>
						</div>
						<div class='col-md-4 mb-6' style='background-color: ".$colors['smbgcolor']."; border-color: ".$colors['smbgcolor']."85;'><span></span></div>
						</div>
						<div class='row form-row'>
						<div class='col-md-8 mb-6'>
							<label for='ssbgcolor'>Sidebar Label Background Color:</label> 
							<div class='input-group'> 
								<div class='input-group-prepend'>
									<span class='input-group-text' id='igp4' style='background-color: ".$colors['ssbgcolor']."; border-color: ".$colors['ssbgcolor']."85;'>@</span>
								</div>
								<input type='text' class='form-control' aria-describedby='igp4' placeholder='".$colors['ssbgcolor']."' value='".$colors['ssbgcolor']."' name='ssbgcolor' id='ssbgcolor' maxlength='7' required>
							</div>
						</div>
						<div class='col-md-4 mb-6' style='background-color: ".$colors['ssbgcolor']."; border-color: ".$colors['ssbgcolor']."85;'><span></span></div>
						</div>
						<div class='row form-row'>
						<div class='col-md-8 mb-6'>
							<label for='mccolor'>Main Text Color:</label> 
							<div class='input-group'> 
								<div class='input-group-prepend'>
									<span class='input-group-text' id='igp5' style='background-color: ".$colors['mccolor']."; border-color: ".$colors['mccolor']."85;'>@</span>
								</div>
								<input type='text' class='form-control' aria-describedby='igp5' placeholder='".$colors['mccolor']."' value='".$colors['mccolor']."' name='mccolor' id='mccolor' maxlength='7' required>
							</div>
						</div>
						<div class='col-md-4 mb-6' style='background-color: ".$colors['mccolor']."; border-color: ".$colors['mccolor']."85;'><span></span></div>
						</div>
						<div class='row form-row'>
						<div class='col-md-8 mb-6'>
							<label for='sbcolor'>Sidebar Text Color:</label> 
							<div class='input-group'> 
								<div class='input-group-prepend'>
									<span class='input-group-text' id='igp6' style='background-color: ".$colors['sbcolor']."; border-color: ".$colors['sbcolor']."85;'>@</span>
								</div>
								<input type='text' class='form-control' aria-describedby='igp6' placeholder='".$colors['sbcolor']."' value='".$colors['sbcolor']."' name='sbcolor' id='sbcolor' maxlength='7' required>
							</div>
						</div>
						<div class='col-md-4 mb-6' style='background-color: ".$colors['sbcolor']."; border-color: ".$colors['sbcolor']."85;'><span></span></div>
						</div>
						<div class='row form-row'>
						<div class='col-md-8 mb-6'>
							<label for='sbcolor'>Link Text Color:</label> 
							<div class='input-group'> 
								<div class='input-group-prepend'>
									<span class='input-group-text' id='igp6' style='background-color: ".$colors['linkcolor']."; border-color: ".$colors['linkcolor']."85;'>@</span>
								</div>
								<input type='text' class='form-control' aria-describedby='igp6' placeholder='".$colors['linkcolor']."' value='".$colors['linkcolor']."' name='linkcolor' id='linkcolor' maxlength='7' required>
							</div>
						</div>
						<div class='col-md-4 mb-6' style='background-color: ".$colors['linkcolor']."; border-color: ".$colors['linkcolor']."85;'><span></span></div>
						</div>
						<div class='row form-row'>
							<div class='col-sm-12 col-md-12 mb-6'>
								<div class='form-check'>
									<input class='form-check-input' type='checkbox' name='overrideDefault' value='checked' id='overrideDefault' ".$isDisabled.">
									<label class='form-check-label' for='overrideDefault'>
										Override Default
									</label>
									<small id='overrideDefaultHelp' class='form-text text-muted'> 
										Overrides the default for users who haven't edited their color or logged-out viewers.
									</small>
								</div>
								<div class='form-check'>
									<input class='form-check-input' type='checkbox' value='checked' name='overrideUserData' id='overrideUserData' ".$isDisabled.">
									<label class='form-check-label' for='overrideUserData'>
										Override User Data
									</label>
									<small id='overrideDefaultHelp' class='form-text text-muted'> 
										Overrides the custom style any user has selected.
									</small>
								</div>
							</div>
						</div>
						<br>
						<div class='row form-row'>
							<div class='col-sm-12 col-md-12 mb-6'>
								<input type='hidden' name='action' value='changecolors'>
								<input type='submit' class='btn btn-primary' value='Update Colors'></button>
								<!--input type='submit' value='Update Colors'-->
							</div>
						</div>
					</form>
				</div>
			</div>
			";

			if(!isset($admin)) {
				$pagecontent = $skincoloredits;
			}
			else {
				$pagecontent = $tournamentlist.$skincoloredits;
			}
		}
		
		$pagetitle = "Control Panel - " . $pagesubtitle;
	}
	
	include("./global.php");
?>