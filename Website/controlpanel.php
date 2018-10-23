<?php
	require_once("./preload.php");

	$pagedescription = "You must be logged in to view this.";
	
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
			if($_POST['subaction'] == "participant") {
				$commit = $db->prepare("UPDATE members SET showdownusername = :showdownusername WHERE uid = :uid");
				$commit->execute(array(
					'showdownusername' => $_POST['showdownusername'],
					'uid' => $_POST['uid']
				));
				$commit = $db->prepare("UPDATE teams SET team = :team, checked = :checked WHERE uid = :uid and tid = :tid");
				$commit->execute(array(
					'team' => $_POST['team'],
					'checked' => $_POST['checked'],
					'uid' => $_POST['uid'],
					'tid' => $_POST['tid']
				));
					
				header('Location: ./controlpanel?action=managetournament&tid=' . $tid . '#uid' . $_POST['uid']);
			}
			elseif($_POST['subaction'] == "battle") {
				$commit = $db->prepare("UPDATE battles SET replay1 = :replay1, replay2 = :replay2, winner = :winner WHERE bid = :bid");
				$commit->execute(array(
					'replay1' => $_POST['replay1'],
					'replay2' => $_POST['replay2'],
					'winner' => $_POST['winner'],
					'bid' => $_POST['bid']
				));
				header('Location: ./controlpanel?action=managetournament&tid=' . $tid . '#bid' . $_POST['bid']);
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
				header('Location: ./controlpanel?action=managetournament&tid=' . $tid);
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
			header('Location: ./controlpanel?action=managetournament&tid=' . $db->lastInsertId());
		}
		elseif($action == "doassignbadges") {

			$success = "&success=false";
			$bgidlist = explode(",",$_POST['bgidlist']);
			$bgidvalues = explode(",",$_POST['bgidvalues']);

			foreach	( $bgidlist as $key => $value) {
				$oldvalue = $bgidvalues[$key];
				$newvalue = (($_POST[$value] != null) ? 1 : 0);
				if($newvalue != $oldvalue) {
					$string = "";

					if($newvalue == 1) {
						$string = "INSERT INTO `userbadges`(`uid`, `bgid`) VALUES (:uid,:bgid)";
					} else {
						$string = "DELETE FROM `userbadges` WHERE `uid` = :uid AND `bgid` = :bgid";
					}

					$db->prepare($string)->execute(array(
						'uid' => $_POST['user'],
						'bgid' => $bgidlist[$key]
					));
					$success = "&success=true";
				}
			}

			header('Location: ./controlpanel?action=assignbadges&user='.$_POST['user'] . $success);
		}
		elseif($action == "donewbadge") {
			$success = false;

			$badge = [
				'bgid' => intval($_POST['bgid']),
				'name' => $_POST['name'],
				'url' => $_POST['url']
			];

			$badgeidcheck_q = $db->prepare("SELECT count(`bgid`) AS 'count' FROM `badges` WHERE `bgid` = :bgid");
			$badgeidcheck_q->execute([
				'bgid' => $badge['bgid']
			]);

			if($badgeidcheck_q->fetch()['count'] == 0) {
				$badge_update_q = $db->prepare("INSERT INTO `badges`(`bgid`, `name`, `url`) VALUES (:bgid,:name,:url)");
				$badge_update_q->execute([
					'bgid' => $badge['bgid'],
					'name' => $badge['name'],
					'url' => $badge['url']
				]);
				$success = true;
			}
			else {
				header('Location: ./controlpanel?action=newbadge&success=dupeid&wbgid='.$badge['bgid'].'&name='.$badge['name'].'&url='.$badge['url']);
				return;
			}

			if($success) {
				header('Location: ./controlpanel?action=newbadge&success=true&bgid='.$badge['bgid']);
			}
			else {
				header('Location: ./controlpanel?action=newbadge&success=false');
			}
		}
		elseif($action == "doeditbadge") {
			$success = "&success=false";

			$old_badge = [ 
				'bgid' => intval($_POST['old_bgid']),
				'name' => $_POST['old_name'],
				'url' => $_POST['old_url']
			];

			$new_badge = [
				'bgid' => intval($_POST['bgid']),
				'name' => $_POST['badgename'],
				'url' => $_POST['badgeurl']
			];

			if(($old_badge['bgid'] != $new_badge['bgid']) || ($old_badge['name'] != $new_badge['name']) || ($old_badge['url'] != $new_badge['url'])){
				if($old_badge['bgid'] != $new_badge['bgid']) {
					$badgeidcheck_q = $db->prepare("SELECT count(`bgid`) AS 'count' FROM `badges` WHERE `bgid` = :bgid");
					$badgeidcheck_q->execute([
						'bgid' => $new_badge['bgid']
					]);

					if($badgeidcheck_q->fetch()['count'] == 0) {
						$badge_update_q = $db->prepare("UPDATE `badges` SET `bgid`= :bgid,`name`=:name,`url`=:url WHERE `bgid` = :old_bgid");
						$badge_update_q->execute([
							'bgid' => $new_badge['bgid'],
							'name' => $new_badge['name'],
							'url' => $new_badge['url'],
							'old_bgid' => $old_badge['bgid']
						]);
						$success = "&success=true";
					}
					else {
						$success = "&success=dupeid";
					}
				}
				else {
					$badge_update_q = $db->prepare("UPDATE `badges` SET `name`=:name,`url`=:url WHERE `bgid` = :bgid");
					$badge_update_q->execute([
						'name' => $new_badge['name'],
						'url' => $new_badge['url'],
						'bgid' => $new_badge['bgid']
					]);
					$success = "&success=true";
				}
			}
			else {
				$success = "&success=false";
			}

			#var_dump($old_badge);
			#var_dump($new_badge);

			header('Location: ./controlpanel?action=editbadges&bgid='. (($success == "&success=dupeid") ? $_POST['old_bgid'] : $_POST['bgid']) . $success);
		}
		elseif($action == "dodeletebadge") {
			$delete_q = $db->prepare("DELETE FROM `badges` WHERE `bgid` = :bgid");
			$delete_q->execute([
				'bgid' => $_POST['bgid']
			]);

			#var_dump($_POST['bgid']);

			header('Location: ./controlpanel?action=editbadges&success=true');
		}
		elseif($action == "managetournament") {
			$pagesubtitle = "Tournament Management [#" . $tid . "]";
			$round = 0;
			$participantlist = "";
			$participantmenu = "";
			$participantname = array();
			$battlelist = "";
			$battlemenu = "";
			$bgcolor = getColorFromDB("mbgcolor", $db, $uid);
			
			$tournament = $db->query("SELECT * FROM tournaments WHERE tid = " . $tid)->fetch();
			$participant_q = $db->query("SELECT * FROM teams LEFT JOIN members ON teams.uid = members.uid WHERE tid = " . $tid . " ORDER BY regdate ASC");
			while($participant = $participant_q->fetch()) {
				$participantlist .= "
				<div class='col-sm-12 col-md-5 text-center' style='background-color: ".$bgcolor."; margin: 15px; border-radius: 25px;'>
					<form method='post' style='margin: 10px;' action='controlpanel' id='uid" . $participant['uid'] . "'>
						<div class='form-row'>
							<div class='col'>
								<div class='form-group'>
									<label for='discordusername'><h6>Discord Username / UID:</h6></label> 
									<div class='form-row'>
										<div class='col'>
											<input class='form-control' type='text' id='discordusername' value=\"" . $participant['discordusername'] . "\" readonly> 
										</div>
										<div class='col'>
											<input class='form-control' type='text' value='<@" . $participant['uid'] . ">' readonly>
										</div>
									</div>
								</div>
							</div>
						</div>
						<div class='form-row'>
							<div class='col'>
								<label for='showdownusername'>Showdown Username:</label> 
								<input class='form-control' type='text' name='showdownusername' id='showdownusername' value=\"" . $participant['showdownusername'] . "\">
							</div>
							<div class='col'>
								<label for='checked'>Team Checked: ". ($participant['checked'] ? "<i class='fas fa-check' style='color: green;'></i>" : "<i class='fas fa-times' style='color: red;'></i>") ."</label> 
								<input class='form-control' type='checkbox' name='checked' id='checked' value='1' " . ($participant['checked'] ? "checked" : "") . ">
							</div>
						</div>
						<hr>
						<div class='form-row'>
							<div class='col'>
								<label for='team'>Showdown Team:</label> 
								<textarea rows=10 class='form-control' name='team' id='team'>" . $participant['team'] . "</textarea><br>
							</div>
						</div>
						<div class='form-row'>
							<div class='col'>
								<div class='form-group'>
									<button class='btn btn-primary' type='submit'>Commit Changes</button>
									<a class='btn btn-dark' type='button' href='https://bulbaleague.soaringnetwork.com/controlpanel'>Cancel</a>
									<input type='hidden' name='action' value='domanagetournament'>
									<input type='hidden' name='subaction' value='participant'>
									<input type='hidden' name='tid' value='" . $tid . "'>
									<input type='hidden' name='uid' value='" . $participant['uid'] . "'>
								</div>
							</div>
						</div>
					</form>
				</div>
				";
				$participantname[$participant['pid']] = $participant['discordusername'];
				$participantmenu .= "
				<li class='list-inline-item' style='margin-bottom: 5px;'>
					<a class='btn btn-info' role='button' href='#uid	" . $participant['uid'] . "'>" . $participant['discordusername'] . "</a>
				</li>
				";
			}
			
			$battle_q = $db->query("SELECT * FROM battles WHERE tid = " . $tid . " ORDER BY round ASC, bid ASC");
			while($battle = $battle_q->fetch()) {
				if($battle['round'] > $round) {
					$round = $battle['round'];
					$battlelist .= "
					</div>
					<div class='row justify-content-sm-center' style='margin: 15px 0 15px 0;'>
						<div class='col-sm-auto text-center'>
							<h4 id='round'><em>Round " . $round . "</em></h4>
							<hr>
						</div>
					</div>
					<div class='row justify-content-sm-center' style='margin: 15px 0 15px 0;'>
					";
				}
				$battlelist .= "
					<div class='col-sm-12 col-md-5 text-center' style='background-color: ".$bgcolor."; margin: 15px; border-radius: 25px;'>
						<form method='post' action='controlpanel' id='bid" . $battle['bid'] . "'>
							<div class='form-row'>
								<div class='col-sm-12 col-md-6'>
									<div class='form-group'>
										<label for='player1'> Player 1: </label> 
										<input class='form-control' type='text' id='player1' value=\"" . $participantname[$battle['player1']] . "\" readonly>
									</div>
								</div>
								<div class='col-sm-12 col-md-6'>
									<div class='form-group'>
										<label for='player2'> Player 2: </label> 
										<input class='form-control' type='text' id='player2' value=\"" . ($battle['player2'] ? $participantname[$battle['player2']] : "Bye") . "\" readonly>
									</div>
								</div>
							</div>
							<div class='form-row'>
								<div class='col-sm-12 col-md-6'>
									<div class='form-group'>
										<label for='replay1'>".substr($participantname[$battle['player1']], 0, -5)."'s Replay:</label> <input class='form-control' type='text' name='replay1' id='replay1' value=\"" . $battle['replay1'] . "\">
									</div>
								</div>
								<div class='col-sm-12 col-md-6'>
									<div class='form-group'>
										<label for='replay2'>".substr($participantname[$battle['player2']], 0, -5)."'s Replay:</label> <input class='form-control' type='text' name='replay2' id='replay2' value=\"" . $battle['replay2'] . "\">
									</div>
								</div>
							</div>
							<div class='form-row'>
								<div class='col'>
									<div class='form-group'>
										<label for='winner'>Winner:</label> 
										<select class='form-control' name='winner' id='winner'>
											<option value='0'>None</option>
											<option value='1' " . ($battle['winner'] == 1 ? "selected" : '') . ">".$participantname[$battle['player1']]."</option>
											<option value='2' " . ($battle['winner'] == 2 ? "selected" : '') . ">".$participantname[$battle['player2']]."</option>
										</select>
									</div>
								</div>
							</div>
							<div class='form-row'>
								<div class='col'>
									<div class='form-group'>
										<button class='btn btn-primary' type='submit'>Commit Changes</button>
										<input type='hidden' name='action' value='domanagetournament'>
										<input type='hidden' name='subaction' value='battle'>
										<input type='hidden' name='tid' value='" . $tid . "'>
										<input type='hidden' name='bid' value='" . $battle['bid'] . "'>
									</div>
								</div>
							</div>
						</form>
					</div>
				";
				$battlemenu .= "
				<li class='list-inline-item' style='margin-bottom: 5px;'>
					<a class='btn btn-info' role='button' href='#bid" . $battle['bid'] . "'>Round " . $round . " <br> " . $participantname[$battle['player1']] . "<br>VS<br>" . ($battle['player2'] ? $participantname[$battle['player2']] : "Bye") . "</a>
				</li>";
			}
			
			$pagecontent = "
			<div class='row justify-content-sm-center'>
				<div class='col-10 col-sm-10' style='background-color: #ffffffa0; border-radius:10px; margin: 10px;'>
					<div class='row' style='margin: 15px 0 15px 0;'>
						<div class='col text-center'>
							<h3><span class='badge badge-dark'>Edit</span> Tournament: <i>".$tournament['tournament']."</i></h3>
							<hr>
						</div>
					</div>
					<div class='row'>
						<div class='col'>
							<div class='inpagemenu'>
								<ul class='nav nav-pills justify-content-center' style='margin: 10px 0 10px 0;'>
									<li class='nav-item'>	
										<a class='btn btn-primary' role='button' href='#tournament'>Tournament Details</a>
									</li>
									<li class='nav-item' style='margin: 0 5px 0 5px;'>
										<a class='btn btn-info' role='button' href='#participant'>Participants</a>
									</li>
									<li class='nav-item'>
										<a class='btn btn-danger' role='button' href='#battle'>Battles</a>
									</li>
								</ul>
							</div>
						</div>
					</div>
					<div class='row justify-content-sm-center' style='margin: 15px 0 15px 0;'>
						<div class='col-sm-auto text-center'>
							<h4>Tournament Details</h4>
							<hr>
						</div>
					</div>
					<div class='row'>
						<div class='col'>
							<strong id='tournament'>Tournament Details</strong><br>
							<br>
							<form method='post' action='controlpanel'>
								<div class='form-row'>
									<div class='col-md-6 col-sm-12'>
										<div class='form-group'>
											<label for='tournament'>Tournament Name:</label> 
											<input type='text' class='form-control' name='tournament' id='tournament' value=\"" . $tournament['tournament'] . "\" required>
										</div>
									</div>
									<div class='col-md-3 col-sm-6 text-center'>
										<div class='form-group'>
											<label for='minplayers'>Minimum # of Players:</label> 
											<input type='text' style='margin: 0 auto;' class='form-control text-center' name='minplayers' id='minplayers' value=\"" . $tournament['minplayers'] . "\" required>
										</div>
									</div>
									<div class='col-md-3 col-sm-6 text-center'>
										<div class='form-group'>
											<label for='maxplayers'>Maximum # of Players:</label> 
											<input type='text' style='margin: 0 auto;' class='form-control text-center' name='maxplayers' id='maxplayers' value=\"" . $tournament['maxplayers'] . "\" required>
										</div>
									</div>
								</div>
								<hr>
								<div class='form-row'>
									<div class='col-md-4 col-sm-12'>
										<div class='form-group'>
											<label for='regstart'>Registrations Opening Date:</label> 
											<div class='input-group date' id='regstartfield' data-target-input='nearest'>
												<input type='text' name='regstart' id='regstart' class='form-control datetimepicker-input' data-target='#regstartfield' data-toggle='datetimepicker' value='" . date('m/d/Y g:i A', $tournament['regstart']) . "' required/>
												<span class='input-group-addon' data-target='#regstartfield' data-toggle='datetimepicker'>
													<span class='fa fa-calendar'></span>
												</span>
											</div>
										</div>
									</div>
									<div class='col-md-4 col-sm-12'>
										<div class='form-group'>
											<label for='regend'>Registrations Closure Date:</label> 
											<div class='input-group date' id='regendfield' data-target-input='nearest'>
												<input type='text' name='regend' id='regend' class='form-control datetimepicker-input' data-target='#regendfield' data-toggle='datetimepicker' value='" . date('m/d/Y g:i A', $tournament['regend']) . "' required/>
												<span class='input-group-addon' data-target='#regendfield' data-toggle='datetimepicker'>
													<span class='fa fa-calendar'></span>
												</span>
											</div>
										</div>
									</div>
									<div class='col-md-4 col-sm-12'>
										<div class='form-group'>
											<label for='closure'>Tournament Closure Date:</label> 
											<div class='input-group date' id='closurefield' data-target-input='nearest'>
												<input type='text' name='closure' id='closure' class='form-control datetimepicker-input' data-target='#closurefield' data-toggle='datetimepicker' value='" . date('m/d/Y g:i A', $tournament['closure']) . "' required/>
												<span class='input-group-addon' data-target='#closurefield' data-toggle='datetimepicker'>
													<span class='fa fa-calendar'></span>
												</span>
											</div>
										</div>
									</div>
								</div>
								<div class='form-row'>
									<div class='col'>
										<div class='form-group'>
											<label for='roundend'>End of Round 1:</label> 
											<div class='input-group date' id='roundendfield' data-target-input='nearest'>
												<input type='text' name='roundend' id='roundend' class='form-control datetimepicker-input' data-target='#roundendfield' data-toggle='datetimepicker' value='" . date('m/d/Y g:i A', $tournament['roundend']) . "' required/>
												<span class='input-group-addon' data-target='#roundendfield' data-toggle='datetimepicker'>
													<span class='fa fa-calendar'></span>
												</span>
											</div>
										</div>
									</div>
									<div class='col'>
										<div class='form-group'>
											<label for='nextround'>Beginning of Round 2:</label>
											<div class='input-group date' id='nextroundfield' data-target-input='nearest'>
												<input type='text' name='nextround' id='nextround' class='form-control datetimepicker-input' data-target='#nextroundfield' data-toggle='datetimepicker' value='" . date('m/d/Y g:i A', $tournament['nextround']) . "' required/>
												<span class='input-group-addon' data-target='#nextroundfield' data-toggle='datetimepicker'>
													<span class='fa fa-calendar'></span>
												</span>
											</div>
										</div>
									</div>
								</div>
								<hr>
								<div class='form-row'>
									<div class='col'>
										<div class='form-group'>
											<label for='e4member1'>Elite 4 Member 1:</label> 
											<select class='form-control' name='e4member1' id='e4member1'>
												<option value='0'>None Selected</option>
												" . e4selectboxoptions($participantname, $tournament['e4member1']) . "
											</select>
										</div>
									</div>
									<div class='col'>
										<div class='form-group'>
											<label for='e4member2'>Elite 4 Member 2:</label> 
											<select class='form-control' name='e4member2' id='e4member2'>
												<option value='0'>None Selected</option>
												" . e4selectboxoptions($participantname, $tournament['e4member2']) . "
											</select>
										</div>
									</div>
									<div class='col'>
										<div class='form-group'>
											<label for='e4member3'>Elite 4 Member 3:</label> 
											<select class='form-control' name='e4member3' id='e4member3'>
												<option value='0'>None Selected</option>
												" . e4selectboxoptions($participantname, $tournament['e4member3']) . "
											</select>
										</div>
									</div>
									<div class='col'>
										<div class='form-group'>
											<label for='e4member4'>Elite 4 Member 4:</label> 
											<select class='form-control' name='e4member4' id='e4member4'>
												<option value='0'>None Selected</option>
												" . e4selectboxoptions($participantname, $tournament['e4member4']) . "
											</select>
										</div>
									</div>
									<div class='col'>
										<div class='form-group'>
											<label for='e4member5'>Elite 4 Champion:</label> 
											<select class='form-control' name='e4member5' id='e4member5'>
												<option value='0'>None Selected</option>
												" . e4selectboxoptions($participantname, $tournament['e4member5']) . "
											</select>
										</div>
									</div>
								</div>
								<br>
								<div class='form-row'>
									<div class='col'>
										<div class='form-group'>
											<button class='btn btn-primary' type='submit'>Commit Changes</button>
											<a class='btn btn-dark' type='button' href='https://bulbaleague.soaringnetwork.com/controlpanel'>Cancel</a>
											<input type='hidden' name='action' value='domanagetournament'>
											<input type='hidden' name='subaction' value='tournament'>
											<input type='hidden' name='tid' value='" . $tid . "'>
										</div>
									</div>
								</div>
							</form>
							<hr>
						</div>
					</div>
					<div class='row justify-content-sm-center' style='margin: 15px 0 15px 0;'>
						<div class='col-sm-auto text-center'>
							<h4 id='participant'>Participants</h4>
							<hr>
						</div>
					</div>
					<div class='row'>
						<div class='col'>
							<div class='inpagemenu'>
								<div class='d-flex'>
									<ul class='list-inline mx-auto justify-content-center text-center' >
										" . $participantmenu . "
									</ul>
								</div>
							</div>
						</div>
					</div>
					<div class='row justify-content-sm-center'>
						" . $participantlist . "
					</div>
					<hr>
					<div class='row justify-content-sm-center' style='margin: 15px 0 15px 0;'>
						<div class='col-sm-auto text-center'>
							<h4 id='battle'>Battles</h4>
							<hr>
						</div>
					</div>
					<div class='row'>
						<div class='col'>
							<div class='inpagemenu'>
								<div class='d-flex'>
									<ul class='list-inline mx-auto justify-content-center text-center'>
										" . $battlemenu . "
									</ul>
								</div>
							</div>
						</div>
					</div>
					<div class='row justify-content-sm-center' style='margin: 15px 0 15px 0;'>	
						" . $battlelist . "
					</div>
				</div>
			</div>
			";
		}
		elseif($action == "newtournament") {
			$pagesubtitle = "New Tournament";
			
			$pagecontent .= "
			<div class='row justify-content-sm-center'>
				<div class='col-10 col-sm-10' style='background-color: #ffffffa0; border-radius:10px; margin: 10px;'>
					<div class='row' style='margin: 15px 0 15px 0;'>
						<div class='col text-center'>
							<h3><span class='badge badge-dark'>New!</span> Create tournament</h3>
							<hr>
						</div>
					</div>
					<form method='post' action='controlpanel'>
						<div class='form-row'>
							<div class='col-md-6 col-sm-12'>
								<div class='form-group'>
									<label for='tournament'>Tournament Name:</label> 
									<input type='text' class='form-control' name='tournament' id='tournament' required>
								</div>
							</div>
							<div class='col-md-3 col-sm-6 text-center'>
								<div class='form-group'>
									<label for='minplayers'>Minimum # of Players:</label> 
									<input type='text' style='margin: 0 auto;' class='form-control text-center' name='minplayers' id='minplayers' required>
								</div>
							</div>
							<div class='col-md-3 col-sm-6 text-center'>
								<div class='form-group'>
									<label for='maxplayers'>Maximum # of Players:</label> 
									<input type='text' style='margin: 0 auto;' class='form-control text-center' name='maxplayers' id='maxplayers' required>
								</div>
							</div>
						</div>
						<hr>
						<div class='form-row'>
							<div class='col-md-4 col-sm-12'>
								<div class='form-group'>
									<label for='regstart'>Registrations Opening Date:</label> 
									<div class='input-group date' id='regstartfield' data-target-input='nearest'>
										<input type='text' name='regstart' id='regstart' class='form-control datetimepicker-input' data-target='#regstartfield' data-toggle='datetimepicker' placeholder='" . date('m/d/Y g:i A') . "' required/>
										<span class='input-group-addon' data-target='#regstartfield' data-toggle='datetimepicker'>
											<span class='fa fa-calendar'></span>
										</span>
									</div>
								</div>
							</div>
							<div class='col-md-4 col-sm-12'>
								<div class='form-group'>
									<label for='regend'>Registrations Closure Date:</label> 
									<div class='input-group date' id='regendfield' data-target-input='nearest'>
										<input type='text' name='regend' id='regend' class='form-control datetimepicker-input' data-target='#regendfield' data-toggle='datetimepicker' placeholder='" . date('m/d/Y g:i A') . "' required/>
										<span class='input-group-addon' data-target='#regendfield' data-toggle='datetimepicker'>
											<span class='fa fa-calendar'></span>
										</span>
									</div>
								</div>
							</div>
							<div class='col-md-4 col-sm-12'>
								<div class='form-group'>
									<label for='closure'>Tournament Closure Date:</label> 
									<div class='input-group date' id='closurefield' data-target-input='nearest'>
										<input type='text' name='closure' id='closure' class='form-control datetimepicker-input' data-target='#closurefield' data-toggle='datetimepicker' placeholder='" . date('m/d/Y g:i A') . "' required/>
										<span class='input-group-addon' data-target='#closurefield' data-toggle='datetimepicker'>
											<span class='fa fa-calendar'></span>
										</span>
									</div>
								</div>
							</div>
						</div>
						<div class='form-row'>
							<div class='col'>
								<div class='form-group'>
									<label for='roundend'>End of Round 1:</label> 
									<div class='input-group date' id='roundendfield' data-target-input='nearest'>
										<input type='text' name='roundend' id='roundend' class='form-control datetimepicker-input' data-target='#roundendfield' data-toggle='datetimepicker' placeholder='" . date('m/d/Y g:i A') . "' required/>
										<span class='input-group-addon' data-target='#roundendfield' data-toggle='datetimepicker'>
											<span class='fa fa-calendar'></span>
										</span>
									</div>
								</div>
							</div>
							<div class='col'>
								<div class='form-group'>
									<label for='nextround'>Beginning of Round 2:</label>
									<div class='input-group date' id='nextroundfield' data-target-input='nearest'>
										<input type='text' name='nextround' id='nextround' class='form-control datetimepicker-input' data-target='#nextroundfield' data-toggle='datetimepicker' placeholder='" . date('m/d/Y g:i A') . "' required/>
										<span class='input-group-addon' data-target='#nextroundfield' data-toggle='datetimepicker'>
											<span class='fa fa-calendar'></span>
										</span>
									</div>
								</div>
							</div>
						</div>
						<hr>
						<div class='form-row'>
							<div class='col'>
								<div class='form-group'>
									<button class='btn btn-primary' type='submit'>Create Tournament</button>
									<a class='btn btn-dark' type='button' href='https://bulbaleague.soaringnetwork.com/controlpanel'>Cancel</a>
									<input type='hidden' name='action' value='donewtournament'>
								</div>
							</div>
						</div>
					</form>
				</div>
			</div>
			";
		}
		elseif($action == "newbadge") {
			$pagesubtitle = "New Badge";

			$pagecontent = "
			<div class='row justify-content-sm-center'>
				<div class='col-10 col-sm-10' style='background-color: #ffffffa0; border-radius:10px; margin: 10px;'>
					".
					(isset($_GET['success']) ? 
						(($_GET['success'] == "true") ? "
							<div class='row' style='padding-top: 10px;'>
								<div class='col-sm-12'>
									<div class='alert alert-success' role='alert'>
										Badge Created!<br>
										You can check it out <a href='https://bulbaleague.soaringnetwork.com/controlpanel?action=editbadges&bgid=".$_GET['bgid']."'>here</a>
									</div>
								</div>
							</div>
						" : 
							(($_GET['success'] == "dupeid") ? "
								<div class='row' style='padding-top: 10px;'>
									<div class='col-sm-12'>
										<div class='alert alert-danger' role='alert'>
											This ID is already in use. Please use a unique ID.
										</div>
									</div>
								</div>
							" : "
								<div class='row' style='padding-top: 10px;'>
									<div class='col-sm-12'>
										<div class='alert alert-danger' role='alert'>
											Unable to create badge.
										</div>
									</div>
								</div>
							")
						) 
					: "")
					."
					<div class='row' style='padding: 10px 0;'>
						<div class='col-sm-12'>
							<h2>Creating new badge!</h2>
						</div>
					</div>
					<form method='post' action='controlpanel'>
						<div class='form-group'>
							<div class='form-row'>
								<div class='col-sm-12 col-md-8'>
									<div class='form-row'>
										<div class='col-sm-12 col-md-6'>
											<label for='bgid'>Badge ID:</label>
											<input class='form-control' type='number' name='bgid' id='bgid' step=1 min=1 required ".((isset($_GET['wbgid'])) ? "value='".$_GET['wbgid']."'" : "").">
											<small class='form-text text-muted'>Numbers 1-28 are reserved. Please don't use them.</small>
										</div>
										<div class='col-sm-12 col-md-6'>
											<label for='name'>Badge Name:</label>
											<input class='form-control' type='text' name='name' id='name' required ".((isset($_GET['name'])) ? "value='".$_GET['name']."'" : "").">
										</div>
									</div>
								</div>
								<div class='col-sm-12 col-md-4'>
									<div class='form-row'>
										<div class='col-sm-12 col-md-12'>
											<label for='url'>Badge URL:</label>
											<input class='form-control' type='url' name='url' id='url' required ".((isset($_GET['url'])) ? "value='".$_GET['url']."'" : "").">
											<small class='form-text text-muted'>Please upload the file to the server first and use the URL.</small>
										</div>
									</div>
								</div>
							</div>
							<div class='form-row' style='padding: 5px 0;'>
								<div class='col-sm-12'>
									<input type='hidden' name='action' value='donewbadge'>
									<input type='submit' class='btn btn-primary' value='Create Badge'>
								</div>
							</div>
							<div class='form-row'>
								<div class='col-sm-12'>
									<a role='button' class='btn btn-danger' href='/controlpanel?action=editbadges'>Cancel</a>
								</div>
							</div>
						</div>
					</form>
				</div>
			</div>
			";
		}
		elseif($action == "editbadges") {
			$pagesubtitle = "Edit Badges";
			
			if(isset($_GET['bgid'])) {
				
				$badge_q = $db->prepare("SELECT * FROM `badges` WHERE `bgid` = :bgid");
				$badge_q->execute([
					'bgid' => $_GET['bgid']
				]);

				$badge = $badge_q->fetch();

				$pagecontent = "
				<div class='row justify-content-sm-center'>
					<div class='col-10 col-sm-10' style='background-color: #ffffffa0; border-radius:10px; margin: 10px;'>
						".
						(isset($_GET['success']) ? 
							(($_GET['success'] == "true") ? "
								<div class='row' style='padding-top: 10px;'>
									<div class='col-sm-12'>
										<div class='alert alert-success' role='alert'>
											Badge successfully edited.
										</div>
									</div>
								</div>
							" : 
								(($_GET['success'] == "dupeid") ? "
									<div class='row' style='padding-top: 10px;'>
										<div class='col-sm-12'>
											<div class='alert alert-danger' role='alert'>
												This ID is already in use. Please use a unique ID.
											</div>
										</div>
									</div>
								" : "
									<div class='row' style='padding-top: 10px;'>
										<div class='col-sm-12'>
											<div class='alert alert-warning' role='alert'>
												No changes detected.
											</div>
										</div>
									</div>
								")
							) 
						: "")
						."
						<div class='row' style='padding: 10px 0;'>
							<div class='col-sm-12'>
								<h2>Editing badge ".$badge['bgid']." (".$badge['name'].")</h2>
							</div>
						</div>
						<form method='post' action='controlpanel'>
							<div class='form-group'>
								<div class='form-row'>
									<div class='col-sm-12 col-md-6'>
										<div class='form-row'>
											<div class='col-sm-12 col-md-6'>
												<label for='bgid'>Badge ID:</label>
												<input class='form-control' type='number' name='bgid' id='bgid' step=1 min=1 required value='".$badge['bgid']."'>
												<small class='form-text text-muted'>Numbers 1-28 are reserved. Please don't use them.</small>
											</div>
											<div class='col-sm-12 col-md-6'>
												<label for='badgename'>Badge Name:</label>
												<input class='form-control' type='text' name='badgename' id='badgename' required value='".$badge['name']."'>
											</div>
										</div>
									</div>
									<div class='col-sm-12 col-md-6'>
										<div class='form-row'>
											<div class='col-sm-12 col-md-8'>
												<label for='badgeurl'>Badge URL:</label>
												<input class='form-control' type='url' name='badgeurl' id='badgeurl' required value='".$badge['url']."'>
												<small class='form-text text-muted'>Please upload the file to the server first and use the URL.</small>
											</div>
											<div class='col-sm-12 col-md-4'>
												<label for='badgeimg'>Current Image:</label>
												<image class='form-control' id='badgeimg' src='".$badge['url']."'>
											</div>
										</div>
									</div>
								</div>
								<div class='form-row' style='padding: 5px 0;'>
									<div class='col-sm-12'>
										<input type='hidden' name='action' value='doeditbadge'>
										<input type='hidden' name='old_bgid' value='".$badge['bgid']."'>
										<input type='hidden' name='old_name' value='".$badge['name']."'>
										<input type='hidden' name='old_url' value='".$badge['url']."'>
										<input type='submit' class='btn btn-primary' value='Submit Changes'>
									</div>
								</div>
								<div class='form-row' style='padding-bottom: 5px;'>
									<div class='col-sm-12'>
										<button type='button' class='btn btn-danger' data-toggle='modal' data-target='#warningDelete'>Delete Badge</button>
									</div>
								</div>
								<div class='form-row'>
									<div class='col-sm-12'>
										<a role='button' class='btn btn-danger' href='/controlpanel?action=editbadges'>Cancel</a>
									</div>
								</div>
							</div>
						</form>

						<!-- Modal -->
						<div class='modal fade' id='warningDelete' tabindex='-1' role='dialog' aria-labelledby='warningDeleteLabel' aria-hidden='true'>
							<div class='modal-dialog' role='document'>
								<div class='modal-content'>
									<div class='modal-header'>
										<h5 class='modal-title' id='warningDeleteLabel'>Delete Badge</h5>
										<button type='button' class='close' data-dismiss='modal' aria-label='Close'>
										<span aria-hidden='true'>&times;</span>
										</button>
									</div>
									<div class='modal-body'>
										Are you sure you want to delete this badge? (Name: ".$badge['name']." - ID: ".$badge['bgid'].")
									</div>
									<div class='modal-footer'>
										<button type='button' class='btn btn-secondary' data-dismiss='modal'>Cancel</button>
										<form method='post' action='controlpanel'>
											<input type='hidden' name='action' value='dodeletebadge'>
											<input type='hidden' name='bgid' value='".$badge['bgid']."'>
											<input type='submit' class='btn btn-danger' value='Delete'>
										<form>
									</div>
								</div>
							</div>
						</div>
						<!-- End of Modal -->

					</div>
				</div>
				";

			} else {
				$paginator = "";
				
				$paginator_q = $db->prepare("SELECT * FROM `badges`");
				$paginator_q->execute();

				while($paginator_e = $paginator_q->fetch()) {
					$paginator .= "
					<tr>
						<td><a href='/controlpanel?action=editbadges&bgid=".$paginator_e['bgid']."'>".$paginator_e['bgid']."</a></td>
						<td><a href='/controlpanel?action=editbadges&bgid=".$paginator_e['bgid']."'>".$paginator_e['name']."</a></td>
						<td><img src='".$paginator_e['url']."'></td>
					</tr>
					";
				}

				$pagecontent .= "
					<div class='row justify-content-sm-center'>
						<div class='col-10 col-sm-10' style='background-color: #ffffffa0; border-radius:10px; margin: 10px;'>
							<div class='row' style='padding: 10px 0;'>
								<div class='col-sm-12'>
									<h2>Select a badge to edit</h2>
								</div>
							</div>
							<div class='row' style='padding-bottom: 10px;'>
								<div class='col-sm-12'>
									<div class='table-responsive' style='width=80%;'>          
										<table id='memberlist' class='table table-striped table-bordered dt-responsive nowrap' cellspacing='0' width='100%'>
											<thead>
												<tr>
													<th>Badge ID</th>
													<th>Badge Name</th>
													<th>Badge Preview</th>
												</tr>
											</thead>
											<tbody>
												".$paginator."
											</tbody>
										</table>
									</div>
									<div class='row' style='padding-bottom: 5px;'>
										<div class='col-sm-12'>
											<a role='button' class='btn btn-success' href='/controlpanel?action=newbadge'>Create new badge</a>
										</div>
									</div>
									<div class='row'>
										<div class='col-sm-12'>
											<a role='button' class='btn btn-danger' href='/controlpanel'>Cancel</a>
										</div>
									</div>
								</div>
							</div>
						</div>
					</div>
					<script>
						$(document).ready(function () {
							$('#memberlist').DataTable({
								\"paging\": true,
								\"searching\": true,
								\"ordering\": true,
								\"info\": false	
							});
						});
					</script>
				";
			}
		}
		elseif($action == "assignbadges") {
			$pagesubtitle = "Assign Badges";
			
			if(isset($_GET['user'])) {
				
				$paginator = "";
			
				$paginator_q = $db->prepare("SELECT `badges`.`bgid`, `badges`.`name`, `badges`.`url`, (CASE WHEN EXISTS( SELECT * FROM `userbadges` WHERE `userbadges`.`uid` = :uid AND `badges`.`bgid`=`userbadges`.`bgid` ) THEN true ELSE false END) AS 'checked' FROM `badges`");
				$paginator_q->execute(array(
					'uid' => $_GET['user']
				));

				$name_q = $db->prepare("SELECT `discordusername` FROM `members` WHERE `uid` = :uid");
				$name_q->execute(['uid' => $_GET['user']]);
				$name = $name_q->fetch()['discordusername'];

				$bgidlist = "<input type='hidden' name='bgidlist' value='";
				$bgidvalues = "<input type='hidden' name='bgidvalues' value='";

				while($paginator_e = $paginator_q->fetch()) {
					$bgidlist .= $paginator_e['bgid'].",";
					$bgidvalues .= $paginator_e['checked'].",";
					$paginator .= "
					<tr>
						<td><input type='checkbox' name='".$paginator_e['bgid']."' ".(($paginator_e['checked']) ? "checked" : "")."> </td>
						<td>".$paginator_e['name']."</td>
						<td><img src='".$paginator_e['url']."'></td>
					</tr>
					";
				}
				$bgidlist .= "'>";
				$bgidvalues .= "'>";

				$pagecontent .= "
				<div class='row justify-content-sm-center'>
					<div class='col-10 col-sm-10' style='background-color: #ffffffa0; border-radius:10px; margin: 10px;'>
					".
					(isset($_GET['success']) ? (($_GET['success'] == "true") ? "
					<div class='row' style='padding-top: 10px;'>
						<div class='col-sm-12'>
							<div class='alert alert-success' role='alert'>
								Badges successfully edited.
							</div>
						</div>
					</div>
					" : "
					<div class='row' style='padding-top: 10px;'>
						<div class='col-sm-12'>
							<div class='alert alert-warning' role='alert'>
								No badges edited.
							</div>
						</div>
					</div>
					") : "")
					."
						<div class='row' style='padding: 10px 0;'>
							<div class='col-sm-12'>
								<h2>Editing ".$name."'s badges</h2>
							</div>
						</div>
						<div class='row' style='padding-bottom: 10px;'>
							<div class='col-sm-12'>
								<form method='post' action='controlpanel'>
									<div class='table-responsive' style='width=80%;'>          
										<table id='memberlist' class='table table-striped table-bordered dt-responsive nowrap' cellspacing='0' width='100%'>
											<thead>
												<tr>
													<th>Has Badge</th>
													<th>Badge Name</th>
													<th>Badge Preview</th>
												</tr>
											</thead>
											<tbody>
												".$paginator."
											</tbody>
										</table>
									</div>
									<input type='hidden' name='action' value='doassignbadges'>
									<input type='hidden' name='user' value='".$_GET['user']."'>
									".$bgidlist.$bgidvalues."
									<div class='form-row' style='padding-bottom: 5px;'>
										<div class='col-sm-12'>
											<input type='submit' class='btn btn-primary' value='Assign Badges'>
										</div>
									</div>
									<div class='form-row'>
										<div class='col-sm-12'>
											<a role='button' class='btn btn-danger' href='/controlpanel?action=assignbadges'>Cancel</a>
										</div>
									</div>
								</form>
							</div>
						</div>
					</div>
				</div>
				<script>
					$(document).ready(function () {
						$('#memberlist').DataTable({
							\"paging\": true,
							\"searching\": true,
							\"ordering\": true,
							\"info\": false	
						});
					});
				</script>
				";

			} else {
				$paginator = "";
			
				$paginator_q = $db->query("SELECT members.`uid`, `discordusername`, `showdownusername`, `tourneyswon`, COUNT(`tid`) AS tourneysparticipated FROM `members` RIGHT JOIN `teams` ON members.`uid` = teams.`uid` GROUP BY `tourneyswon`, `discordusername`, `showdownusername`, members.`uid`");
				while($paginator_e = $paginator_q->fetch()) {
					$paginator .= "
					<tr>
						<td><a href='/controlpanel?action=assignbadges&user=".$paginator_e['uid']."'>".$paginator_e['discordusername']."</a></td>
						<td><a href='/controlpanel?action=assignbadges&user=".$paginator_e['uid']."'>".$paginator_e['showdownusername']."</a></td>
						<td><a href='/controlpanel?action=assignbadges&user=".$paginator_e['uid']."'>".$paginator_e['tourneysparticipated']."</a></td>
						<td><a href='/controlpanel?action=assignbadges&user=".$paginator_e['uid']."'>".$paginator_e['tourneyswon']."</a></td>
					</tr>
					";
				}

				$pagecontent .= "
				<div class='row justify-content-sm-center'>
					<div class='col-10 col-sm-10' style='background-color: #ffffffa0; border-radius:10px; margin: 10px;'>
						<div class='row' style='padding: 10px 0;'>
								<div class='col-sm-12'>
									<h2>Pick a user to edit their badges</h2>
								</div>
							</div>
							<div class='row' style='padding-bottom: 10px;'>
								<div class='col-sm-12'>
									<div class='table-responsive' style='width=80%;'>          
										<table id='memberlist' class='table table-striped table-bordered dt-responsive nowrap' cellspacing='0' width='100%'>
											<thead>
												<tr>
													<th>Discord Username</th>
													<th>Showdown Username</th>
													<th>Tournaments Participated</th>
													<th>Tournaments Won</th>
												</tr>
											</thead>
											<tbody>
												".$paginator."
											</tbody>
										</table>
									</div>
									<div class='row' style='padding-top: 5px;'>
										<div class='col-sm-12'>
											<a role='button' class='btn btn-danger' href='/controlpanel'>Cancel</a>
										</div>
									</div>
								</div>
							</div>
						</div>
					</div>
				</div>
				<script>
					$(document).ready(function () {
						$('#memberlist').DataTable({
							\"paging\": true,
							\"searching\": true,
							\"ordering\": true,
							\"info\": false	
						});
					});
				</script>
				";
			}
		}
		elseif($action == "changecolors") {

			$pagetitle = "Saving...";
			$pagecontent = "<a href='https://bulbaleague.soaringnetwork.com/controlpanel'>Click here if the page doesn't load...</a>";
			
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

			header("Location: ./controlpanel");
		}
		else {
			$pagesubtitle = "Tournament List";
			$bgcolor = getColorFromDB("mbgcolor", $db, $uid);
			$tournamentlist = "
				<div class='row'>
					<div class='col-12 col-sm-12 col-md-6 col-lg-6 col-xl-6' style='background-color: #ffffffa0'>
						<div class='text-center' style='padding: 10px 0 10px 0; background-color: ".$bgcolor."; border-radius:15px; margin: 20px 0 20px 0;'>
						<ul class='nav justify-content-center nav-pills mb-3' id='pills-tab' role='tablist'>
							<li class='nav-item'>
								<a class='nav-link active' id='pills-tourney-tab' data-toggle='pill' href='#pills-tourney' role='tab' aria-controls='pills-tourney' aria-selected='true'>Tourney</a>
							</li>
							<li class='nav-item'>
								<a class='nav-link' id='pills-badges-tab' data-toggle='pill' href='#pills-badges' role='tab' aria-controls='pills-Badges' aria-selected='false'>Badges</a>
							</li>
						</ul>
						<div class='tab-content' id='pills-tabContent'>
							<div class='tab-pane fade show active' id='pills-tourney' role='tabpanel' aria-labelledby='pills-tourney-tab'>
								<a class='btn btn-success' role='button' href='./controlpanel?action=newtournament'>New Tournament</a>
								<a class='btn btn-primary disabled' role='button' aria-disabled='true' hidden>second button</a> 
							</div>
							<div class='tab-pane fade' id='pills-badges' role='tabpanel' aria-labelledby='pills-badges-tab'>
								<a class='btn btn-warning' role='button' href='/controlpanel?action=editbadges'>Edit Badges</a>
								<a class='btn btn-info' role='button' href='/controlpanel?action=assignbadges'>Assign Badges</a>
							</div>
						</div>
					</div>
							
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
				elseif($tournament['status'] == "Closed") {
					$tournament['imgstyle'] = "style='opacity: 0.5; filter: alpha(opacity=50);'";
				}
				else {
					$tournament['round'] = "";
					$tournament['imgstyle'] = "";
				}
				$tournamentsublist[$tournament['status']] .= "
				<div class='col-sm-12 col-md-12 col-lg-6 col-xl-4' style='padding-bottom:10px;'>
					<div class='card card-center' style='width: 98%; max-width: 189px; border-radius: 15px;'>
						<a href='./controlpanel?action=managetournament&tid=" . $tournament['tid'] . "'><img ".$tournament['imgstyle']." style='height: auto; max-width: 100%;' class='card-img-top' src='https://cdn.bulbagarden.net/upload/9/91/League_logo.png' alt='Card image cap'></a>
						<div class='card-body'>
							<h5><a class='card-title' href='./controlpanel?action=managetournament&tid=" . $tournament['tid'] . "'><span class='badge badge-info'># " . $tournament['tid'] . "</span> " . $tournament['tournament'] . "</a></h5>
							".($tournament['status'] != "Closed" ? "<p class='card-text'> Status: " . $tournament['status'] . $tournament['round'] . "</p> " : "" ). "
						</div>
					</div>
				</div>
				";
			}
			
			$tournamentlist .= "
			" . ($tournamentsublist['Round'] ? "
			<div style='background-color: ".$bgcolor."; border-radius:15px; padding:10px; margin-bottom:20px;'>
				<div class='row justify-content-sm-center'>
					<div class='col-sm-auto text-center'>
						<strong>Running Tournaments</strong>
						<hr>
					</div>
				</div>
				<div class='row'> 
				" . $tournamentsublist['Round'] . "
				</div>
			</div> " : "") . "
			" . ($tournamentsublist['Registrations Open'] ? "
			<div style='background-color: ".$bgcolor."; border-radius:15px; padding:10px; margin-bottom:20px;'>
				<div class='row justify-content-sm-center'>
					<div class='col-sm-auto text-center'>
						<strong>Tournaments Open for Registration</strong>
						<hr>
					</div>
				</div>
				<div class='row'> 
				" . $tournamentsublist['Registrations Open'] . "
				</div>
			</div> " : "") . "
			" . ($tournamentsublist['Planned'] ? "
			<div style='background-color: ".$bgcolor."; border-radius:15px; padding:10px; margin-bottom:20px;'>
				<div class='row justify-content-sm-center'>
					<div class='col-sm-auto text-center'>
						<strong>Planned Tournaments</strong>
						<hr>
					</div>
				</div>
				<div class='row'> 
				" . $tournamentsublist['Planned'] . "
				</div>
			</div>" : "") . "
			" . ($tournamentsublist['Closed'] ? "
				<div style='background-color: ".$bgcolor."; border-radius:15px; padding:10px; margin-bottom:20px;'>
					<div class='row justify-content-sm-center'>
						<div class='col-sm-auto text-center'>
							<strong>Past Tournaments</strong>
							<!--div class='collapseButton'-->
								<button style='font-size: 1em; margin-left: 10px;' type='button' class='btn btn-outline-info' data-toggle='collapse' data-target='#closedtourney' aria-expanded='true' aria-controls='#closedtourney'>v</button>
							<!--/div-->
							<hr>
						</div>
					</div>
					<div class='collapse' id='closedtourney'>
						<div class='row'>
						" . $tournamentsublist['Closed'] . "
						</div>
					</div>
				</div>" : "") . "
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
			else {
				$isDisabled = "";
			}
			$skincoloredits = "
				</div>
				<div class='col-12 col-sm-12 col-md-6 col-lg-6 col-xl-6' style='background-color: #ffffffb0; padding-bottom: 5px;'>
				<style>.form-row {padding-bottom:10px;}</style>
					<strong>Color Settings:</strong><br><br>

					<form method='post' action='controlpanel'>
						<div class='row form-row'>
						<!--form method='post' action='controlpanel'-->
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
								<input type='submit' class='btn btn-primary' value='Update Colors'>
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
		$pagecontent .= "<script>
		$(function () {
			$('#regstartfield').datetimepicker({
				sideBySide: true
			});
		});
		$(function () {
			$('#regendfield').datetimepicker({
				sideBySide: true
			});
		});
		$(function () {
			$('#closurefield').datetimepicker({
				sideBySide: true
			});
		});
		$(function () {
			$('#roundendfield').datetimepicker({
				sideBySide: true
			});
		});
		$(function () {
			$('#nextroundfield').datetimepicker({
				sideBySide: true
			});
		});
		</script>";
	}
	
	include("./global.php");
?>
