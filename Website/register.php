<?php
	require_once("./preload.php");
	
	if(!isset($uid)) {
		$pagetitle = "Access Denied";
		$pagecontent = "
			In order to access this page, you need to be logged in.
		";
		$pagedescription = $pagecontent;
	}
	
	else {
		$notice = "";
		if(isset($_GET['action'])) {
			if($_GET['action'] == "unregister") {
				if($tournament = $db->query("SELECT * FROM tournaments WHERE tid = '" . dbesc($_GET['tid']) . "'")->fetch()) {
					if(time() >= $tournament['regstart'] AND time() <= $tournament['regend']) {
						$db->query("DELETE FROM teams WHERE tid = '" . dbesc($_GET['tid']) . "' AND uid = " . $uid);
						$notice = "<div class='alert alert-info'>Your registration has been cancelled.</div>";
					}
					else {
						$notice = "<div class='alert alert-warning'>Registrations for this tournament are not open.</div>";
					}
				}
				else {
					$notice = "<div class='alert alert-danger'>The requested tournament does not exist.</div>";
				}
			}
		}
		if(isset($_POST['tid'])) {
			if($tournament = $db->query("SELECT * FROM tournaments WHERE tid = '" . dbesc($_POST['tid']) . "'")->fetch()) {
				$registration = $db->query("SELECT * FROM teams LEFT JOIN members ON teams.uid = members.uid WHERE tid = " . $tournament['tid'] . " AND teams.uid = " . $uid)->fetch();
				if(time() >= $tournament['regstart'] AND time() <= $tournament['regend']) {
					if($registration) {
						$db->query("UPDATE members SET discordusername = '" . dbesc($fullname) . "', showdownusername = '" . dbesc($_POST['showdown']) . "', avatar = '" . dbesc($avatar) . "' WHERE uid = " . $uid);
						$db->query("UPDATE teams SET team = '" . dbesc($_POST['team']) . "', checked = NULL WHERE tid = '" . dbesc($_POST['tid']) . "' AND uid = " . $uid);
						$notice = "<div class='alert alert-success'>Your registration details have been updated.</div>";
					}
					else {
						$db->query("INSERT INTO members(uid, discordusername, showdownusername, avatar) VALUES(" . $uid . ", '" . dbesc($fullname) . "', '" . dbesc($_POST['showdown']) . "', '" . dbesc($avatar) . "') ON DUPLICATE KEY UPDATE discordusername = VALUES(discordusername), showdownusername = VALUES(showdownusername), avatar = VALUES(avatar)");
						$db->query("INSERT INTO teams(tid, uid, team, regdate, checked) VALUES('" . dbesc($_POST['tid']) . "', " . $uid . ", '" . dbesc($_POST['team']) . "', " . time() . ", NULL)");
						$notice = "<div class='alert alert-success'>Your registration details have been saved.</div>";
					}
				}
				else {
					$notice = "<div class='alert alert-danger'>Registrations for this tournament are not open.</div>";
				}
			}
			else {
				$notice = "<div class='alert alert-danger'>The requested tournament does not exist.</div>";
			}
		}
		
		$tournamentlist = "";
		$tournament_q = $db->query("SELECT * FROM tournaments WHERE closure >= " . time() . " ORDER BY regstart DESC");
		while($tournament = $tournament_q->fetch()) {
			if(time() < $tournament['regstart']) {
				$tournamentlist .= "
					<div class='tournament'>
						<div class='form-group row align-items-center'>
							<div class='col-sm-3'>
								<big class='form-text font-weight-bold'>".$tournament['tournament']."</big>
							</div>
							<div class='col-sm-9'>
								<span class='form-text alert alert-warning'>
									Registrations for this tournament have not started yet.
									<small class='form-text text-muted'>
										Registrations open: " . fdate($tournament['regstart']) . "<br>
										Registrations close: " . fdate($tournament['regend']) . "
									</small>
								</span>
							</div>
						</div>
					</div>
				";
			}
			else {
				$registration = $db->query("SELECT * FROM teams LEFT JOIN members ON teams.uid = members.uid WHERE tid = " . $tournament['tid'] . " AND teams.uid = " . $uid)->fetch();
				$registered = $db->query("SELECT COUNT(uid) FROM teams WHERE tid = " . $tournament['tid'])->fetch();
				if($registration) {
					$regposition = $db->query("SELECT COUNT(uid) + 1 FROM teams WHERE tid = " . $tournament['tid'] . " AND regdate < " . $registration['regdate'])->fetch();
					if($regposition['COUNT(uid) + 1'] > $tournament['maxplayers']) {
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
					$plq = $db->query("SELECT * FROM teams LEFT JOIN members ON teams.uid = members.uid WHERE tid = " . $tournament['tid'] . " ORDER BY regdate ASC LIMIT 0, " . $tournament['maxplayers']);
					$playerlist = "
					<div class='tournament table-responsive' style='overflow: hidden;'>
						<span class='form-text alert alert-warning'>Work in Progress</span>
						<table class='table table-sm'>
						<thead>
							<tr>
								<th class='font-weight-bold' scope='col'>Discord Name</th>
								<th class='font-weight-bold' scope='col'>Showdown Name</th>
							</tr>
						</thead>
						<tbody >
					";
					while($playerlistdata = $plq->fetch()) {
						$playerlist .= "<tr><th scope='col' class='font-weight-normal'>" . $playerlistdata['discordusername'] . "</th><th scope='col' class='font-weight-normal'>" . $playerlistdata['showdownusername'] . "<th>";
					}
					$playerlist .= "</tbody></table></div>";
					
					$tournamentlist .= "<div class='row'>
					<div class='col-12 col-sm-12 col-md-8 col-lg-8 col-xl-8'>
						<div class='tournament'>
							<form method='post' action='register'>
								<div class='form-group row align-items-center'>
									<big class='form-text font-weight-bold col-sm-3'>".$tournament['tournament']."</big>
									<div class='col-sm-9'>
										<span class='form-text alert alert-info'>".$regmsg."</span>
										<span class='form-text alert alert-success'>
										Registrations for this tournament are currently open, fill the form below to " . ($registration ? "edit your registration details" : "register") . "
										</span>
									</div>
								</div>
								<div class='form-group row align-items-center'>
									<div class='col-sm-3'>
										<label for='discord' class='col-form-label'>Discord Username:</label>
									</div>
									<div class='col-sm-9'>
										<input type='text' class='form-control' name='discord' id='discord' value=\"" . esc($fullname) . "\" readonly>
									</div>
								</div>
								<div class='form-group row align-items-center'>
									<div class='col-sm-3'>
										<label for='showdown' class='col-form-label'>Showdown Username:</label>
									</div>
									<div class='col-sm-9'>
										<input type='text' class='form-control' name='showdown' id='showdown' value=\"" . esc($registration['showdownusername']) . "\" maxlength='18'>
									</div>
								</div>
								<div class='form-group row align-items-center'>
									<div class='col-sm-3'>
										<label for='team' class='col-form-label'>Team:</label>
									</div>
									<div class='col-sm-9'>
									<textarea name='team' class='form-control' id='team' cols='44' rows='8'>" . $registration['team'] . "</textarea>
									</div>
								</div>
								<div class='form-group row align-items-center'>
									<input type='hidden' name='tid' value='" . $tournament['tid'] . "'>
									<div class='col-sm-5'>
										<input type='submit' class='form-control btn btn-primary' value='" . ($registration ? "Edit Registration Details" : "Register") . "'>
										" . ($registration ? "<br><a href='register?action=unregister&tid=" . $tournament['tid'] . "' class='btn btn-danger form-control' role='button'>Cancel Registration</a>" : "") . "
									</div>
									<div class='col-sm-7'>
										<small class='form-text text-muted'>Participants registered: " . $registered['COUNT(uid)'] . "/" . $tournament['maxplayers'] . "</small>
										<span>
										Registrations close: " . fdate($tournament['regend']) . "
										</span>
									</div>
								</div>
							</form>
						</div>
					</div>
					<div class='col-12 col-sm-12 col-md-4 col-lg-4 col-xl-4'>
						".$playerlist."
					</div>
					</div>
					";
				}
				else {
					if($registration) {
						$tournamentlist .= "
						<div class='tournament'>
						<form method='post' action='register'>
							<div class='form-group row align-items-center'>
								<big class='form-text font-weight-bold col-sm-3'>".$tournament['tournament']."</big>
								<div class='col-sm-9'>
									<span class='form-text alert alert-info'>".$regmsg."</span>
									<span class='form-text alert alert-danger'>
										Registrations for this tournament are closed, you can see your registration details below, but not edit them.
									</span>
								</div>
							</div>
							<div class='form-group row align-items-center'>
								<div class='col-sm-3'>
									<label for='discord' class='col-form-label'>Discord Username:</label>
								</div>
								<div class='col-sm-9'>
									<input type='text' class='form-control' name='discord' id='discord' value=\"" . esc($fullname) . "\" readonly>
								</div>
							</div>
							<div class='form-group row align-items-center'>
								<div class='col-sm-3'>
									<label for='showdown' class='col-form-label'>Showdown Username:</label>
								</div>
								<div class='col-sm-9'>
									<input type='text' class='form-control' name='showdown' id='showdown' value=\"" . esc($registration['showdownusername']) . "\" maxlength='18' readonly>
								</div>
							</div>
							<div class='form-group row align-items-center'>
								<div class='col-sm-3'>
									<label for='team' class='col-form-label'>Team:</label>
								</div>
								<div class='col-sm-9'>
								<textarea name='team' class='form-control' id='team' cols='44' rows='8' readonly>" . $registration['team'] . "</textarea>
								</div>
							</div>
							<div class='form-group row align-items-center'>
								<div class='col-sm-3'></div>	
								<div class='col-sm-8'>
									<small class='form-text text-muted'>Participants registered: " . $registered['COUNT(uid)'] . "/" . $tournament['maxplayers'] . "</small>
									<span>
									Registrations close: " . fdate($tournament['regend']) . "
									</span>
								</div>
							</div>
						</form>
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
		<div class='row'>
			<div class='col-12 col-sm-12 col-md-8 col-lg-8 col-xl-8'>
				" . $notice . "
			</div>
		</div>
		<div class='row'>
			<div class='col-12 col-sm-12'>
				" . $tournamentlist . "
			</div>
		</div>
		";
	}
	
	include("./global.php");
?>
