<?php
	require_once("./preload.php");
	
	$pagedescription = "The bulbaleague is a community tournament hosted by the Bulbagarden Public Discord Staff.";
	$pagetitle = "Members";

	//Info Banner
	$notice = "";

	if(isset($_GET['uid'])) {
		$userid = $_GET['uid'];
	}
	elseif(isset($_POST['uid'])) {
		$userid = $_POST['uid'];
	}
	if(isset($_GET['uname'])) {
		$uname = $_GET['uname'];
	}
	elseif(isset($_POST['uname'])) {
		$uname = $_POST['uname'];
	}

	$paginator = "";

		// for savekeeping:
		// SELECT `uid`, `discordusername`, `showdownusername`, COUNT(`tid`) AS tourneysparticipated FROM `participants` WHERE `checked` = true GROUP BY `uid`, `discordusername`, `showdownusername`

	$paginator_q = $db->query("SELECT members.`uid`, `discordusername`, `showdownusername`, `tourneyswon`, COUNT(`tid`) AS tourneysparticipated FROM `members` RIGHT JOIN `teams` ON members.`uid` = teams.`uid` GROUP BY `tourneyswon`, `discordusername`, `showdownusername`, members.`uid`");
	while($paginator_e = $paginator_q->fetch()) {
		$paginator .= "
		<tr>
			<td><a href='/members?uid=".$paginator_e['uid']."'>".$paginator_e['discordusername']."</a></td>
			<td><a href='/members?uid=".$paginator_e['uid']."'>".$paginator_e['showdownusername']."</a></td>
            <td><a href='/members?uid=".$paginator_e['uid']."'>".$paginator_e['tourneysparticipated']."</a></td>
            <td><a href='/members?uid=".$paginator_e['uid']."'>".$paginator_e['tourneyswon']."</a></td>
		</tr>
		";
	}

	$colors = array(
		"mbgcolor" => getColorFromDB("mbgcolor", $db, $uid),
		"hbgcolor" => getColorFromDB("hbgcolor", $db, $uid),
		"smbgcolor" => getColorFromDB("smbgcolor", $db, $uid),
		"ssbgcolor" => getColorFromDB("ssbgcolor", $db, $uid),
		"mccolor" => getColorFromDB("mccolor", $db, $uid),
		"sbcolor" => getColorFromDB("sbcolor", $db, $uid),
		"linkcolor" => getColorFromDB("linkcolor", $db, $uid)
	);

	if(isset($uname))
	{
		$user_q = $db->prepare("SELECT members.`uid`, `timezone`, `avatar`, `discordusername`, `showdownusername`, `tourneyswon`, COUNT(`tid`) AS tourneysparticipated FROM `members` RIGHT JOIN `teams` ON members.`uid` = teams.`uid` WHERE members.`discordusername` LIKE :name OR members.showdownusername LIKE :name GROUP BY members.uid, timezone, avatar, discordusername, showdownusername, tourneyswon ");
		$user_q->execute(array(
			'name' => '%'.$uname.'%'
		));
		$user = $user_q->fetch();

		if(($user['discordusername'] != $uname && $user['showdownusername'] != $uname)) {
			header('Location: https://bulbaleague.soaringnetwork.com/members?uname='.$user['showdownusername']);
		}

		$badges_q = $db->prepare(
			"SELECT `badges`.`url`, `badges`.`name`
			FROM `userbadges`
			RIGHT JOIN `badges` ON `userbadges`.`bgid` = `badges`.`bgid`
			WHERE `userbadges`.`uid` = :uid"
		);
		$badges_q->execute(array(
			'uid' => $user['uid']
		));
		while($badge = $badges_q->fetch()) {
			$badges .= "
				<img style='height: 40px; max-width: 200px; max-height: 40px;' src='".$badge['url']."' title='".$badge['name']."' onclick='infoText(this)'></img>
			";
		}
	
    	$pagetitle = $user['showdownusername']."'s Profile";
		$pagecontent = $notice."
			<div class='row'>
				<div class='col-sm-12 col-md-5' style='word-wrap: break-word; margin-bottom: 10px;'>
					<div style='width: 100%; background-color: #ffffffb0; border-radius: 15px; padding: 5px 5px 15px 5px;'>
						".(($user['discordusername'] != null) ? "
							<div class='row' >
								<div class='col-xs-12 col-xl-3' style='margin-left: 5px; margin-right: -5px;'>
									<div style='margin: 0 auto; max-width: 100%; width: 100px; padding: 10px 0;'>
										<img style='border-radius: 50%; min-width: 100px; min-height: 100px; max-width: 100%; max-height: 100px; margin: 0 auto;' src='".$user['avatar']."'></img>
									</div>
								</div>
								<div class='col-sm-12 col-xl-9'>
									<div style='padding: 10px 0;'>
										<table class='table table-sm' style='word-wrap: break-word;'>
											<tbody>
												<tr>
													<td><b>Discord Username:</b></td>
													<td>".$user['discordusername']."</td>
												</tr>
												<tr>
													<td><b>Showdown Username:</b></td>
													<td>".$user['showdownusername']."</td>
												</tr>
												<tr>
													<td><b>Timezone:</b></td>
													<td>". (($user['timezone'] != null)  ? $user['timezone'] : "Unknown") . "</td>
												</tr>
												<tr>
													<td><b>Tourneys Participated:</b></td>
													<td>".$user['tourneysparticipated']."</td>
												</tr>
												<tr>
													<td><b>Tourneys Won:</b></td>
													<td>".$user['tourneyswon']."</td>
												</tr>
											</tbody>
										</table>
									</div>
								</div>
							</div>
							<hr>
							".
							//TODO: Add badges
							"
							<div class='row'>
								<div class='col-sm-12' style='margin-left:5px;margin-right:-5px;'>
									<h3>Badges</h3>
								</div>
							</div>
							<div class='row'>
								<div class='col-sm-12' style='margin-left:5px;margin-right:-5px;'>
									".$badges."
								</div>
							</div>
							<div class='row'>
								<div class='col-sm-12' style='margin-left:5px;margin-right:-5px;'>
									<span id='infotext'></span>
								</div>
							</div>
							"."
						" : "
							<div class='row'>
								<div class='col-sm-12'>
									<h4>User by username: \"".$uname."\" not found.</h4>
								</div>
							</div>
						")."
					</div>
				</div>
				<div class='col-sm-12 col-md-7' >
					<div style='width: 100%; background-color: #ffffffb0; border-radius: 15px; padding: 5px;'>
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
					</div>
				</div>
			</div>
			<div class='row'>
				<div style='padding-bottom: 50px;'></div>
			</div>
		";
	}
	elseif(isset($userid)) {

		$user_q = $db->prepare("SELECT members.`uid`, `timezone`, `avatar`, `discordusername`, `showdownusername`, `tourneyswon`, COUNT(`tid`) AS tourneysparticipated FROM `members` RIGHT JOIN `teams` ON members.`uid` = teams.`uid` WHERE members.`uid` = :uid");
		$user_q->execute(array(
			'uid' => $userid
		));
		$user = $user_q->fetch();

		$badges_q = $db->prepare(
			"SELECT `badges`.`url`, `badges`.`name`
			FROM `userbadges`
			RIGHT JOIN `badges` ON `userbadges`.`bgid` = `badges`.`bgid`
			WHERE `userbadges`.`uid` = :uid"
		);
		$badges_q->execute(array(
			'uid' => $userid
		));
		while($badge = $badges_q->fetch()) {
			$badges .= "
				<img style='height: 40px; max-width: 200px; max-height: 40px;' src='".$badge['url']."' title='".$badge['name']."' onclick='infoText(this)'></img>
			";
		}
	
    	$pagetitle = $user['showdownusername']."'s Profile";
		$pagecontent = $notice."
			<div class='row'>
				<div class='col-sm-12 col-md-5' style='word-wrap: break-word; margin-bottom: 10px;'>
					<div style='width: 100%; background-color: #ffffffb0; border-radius: 15px; padding: 5px 5px 15px 5px;'>
						".(($user['discordusername'] != null) ? "
							<div class='row' >
								<div class='col-xs-12 col-xl-3' style='margin-left: 5px; margin-right: -5px;'>
									<div style='margin: 0 auto; max-width: 100%; width: 100px; padding: 10px 0;'>
										<img style='border-radius: 50%; min-width: 100px; min-height: 100px; max-width: 100%; max-height: 100px; margin: 0 auto;' src='".$user['avatar']."'></img>
									</div>
								</div>
								<div class='col-sm-12 col-xl-9'>
									<div style='padding: 10px 0;'>
										<table class='table table-sm' style='word-wrap: break-word;'>
											<tbody>
												<tr>
													<td><b>Discord Username:</b></td>
													<td>".$user['discordusername']."</td>
												</tr>
												<tr>
													<td><b>Showdown Username:</b></td>
													<td>".$user['showdownusername']."</td>
												</tr>
												<tr>
													<td><b>Timezone:</b></td>
													<td>". (($user['timezone'] != null)  ? $user['timezone'] : "Unknown") . "</td>
												</tr>
												<tr>
													<td><b>Tourneys Participated:</b></td>
													<td>".$user['tourneysparticipated']."</td>
												</tr>
												<tr>
													<td><b>Tourneys Won:</b></td>
													<td>".$user['tourneyswon']."</td>
												</tr>
											</tbody>
										</table>
									</div>
								</div>
							</div>
							<hr>
							".
							//TODO: Add badges
							"
							<div class='row'>
								<div class='col-sm-12' style='margin-left:5px;margin-right:-5px;'>
									<h3>Badges</h3>
								</div>
							</div>
							<div class='row'>
								<div class='col-sm-12' style='margin-left:5px;margin-right:-5px;'>
									".$badges."
								</div>
							</div>
							<div class='row'>
								<div class='col-sm-12' style='margin-left:5px;margin-right:-5px;'>
									<span id='infotext'></span>
								</div>
							</div>
							"."
						" : "
							<div class='row'>
								<div class='col-sm-12'>
									<h4>User by UID: \"".$userid."\" not found.</h4>
								</div>
							</div>
						")."
					</div>
				</div>
				<div class='col-sm-12 col-md-7' >
					<div style='width: 100%; background-color: #ffffffb0; border-radius: 15px; padding: 5px;'>
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
					</div>
				</div>
			</div>
			<div class='row'>
				<div style='padding-bottom: 50px;'></div>
			</div>
		";
	}
	elseif(isset($uid)) {

		$user_q = $db->prepare("SELECT members.`uid`, `timezone`, `avatar`, `discordusername`, `showdownusername`, `tourneyswon`, COUNT(`tid`) AS tourneysparticipated FROM `members` RIGHT JOIN `teams` ON members.`uid` = teams.`uid` WHERE members.`uid` = :uid");
		$user_q->execute(array(
			'uid' => $uid
		));
		$user = $user_q->fetch();

		$badges_q = $db->prepare(
			"SELECT `badges`.`url`, `badges`.`name`
			FROM `userbadges`
			RIGHT JOIN `badges` ON `userbadges`.`bgid` = `badges`.`bgid`
			WHERE `userbadges`.`uid` = :uid"
		);
		$badges_q->execute(array(
			'uid' => $uid
		));
		while($badge = $badges_q->fetch()) {
			$badges .= "
				<img style='height: 40px; max-width: 200px; max-height: 40px;' src='".$badge['url']."' title='".$badge['name']."' onclick='infoText(this)'></img>
			";
		}

		$pagecontent = $notice."
			<div class='row'>
				<div class='col-sm-12 col-md-5' style='word-wrap: break-word; margin-bottom: 10px;'>
					<div style='width: 100%; background-color: #ffffffb0; border-radius: 15px; padding: 5px 5px 15px 5px;'>
						".(($user['discordusername'] != null) ? "
							<div class='row' >
								<div class='col-xs-12 col-xl-3' style='margin-left: 5px; margin-right: -5px;'>
									<div style='margin: 0 auto; max-width: 100%; width: 100px; padding: 10px 0;'>
										<img style='border-radius: 50%; min-width: 100px; min-height: 100px; max-width: 100%; max-height: 100px; margin: 0 auto;' src='".$user['avatar']."'></img>
									</div>
								</div>
								<div class='col-sm-12 col-xl-9'>
									<div style='padding: 10px 0;'>
										<table class='table table-sm' style='word-wrap: break-word;'>
											<tbody>
												<tr>
													<td><b>Discord Username:</b></td>
													<td>".$user['discordusername']."</td>
												</tr>
												<tr>
													<td><b>Showdown Username:</b></td>
													<td>".$user['showdownusername']."</td>
												</tr>
												<tr>
													<td><b>Timezone:</b></td>
													<td>". (($user['timezone'] != null)  ? $user['timezone'] : "Unknown") . "</td>
												</tr>
												<tr>
													<td><b>Tourneys Participated:</b></td>
													<td>".$user['tourneysparticipated']."</td>
												</tr>
												<tr>
													<td><b>Tourneys Won:</b></td>
													<td>".$user['tourneyswon']."</td>
												</tr>
											</tbody>
										</table>
									</div>
								</div>
							</div>
							<hr>
							".
							//TODO: Add badges
							"
							<div class='row'>
								<div class='col-sm-12' style='margin-left:5px;margin-right:-5px;'>
									<h3>Badges</h3>
								</div>
							</div>
							<div class='row'>
								<div class='col-sm-12' style='margin-left:5px;margin-right:-5px;'>
									".$badges."
								</div>
							</div>
							<div class='row'>
								<div class='col-sm-12' style='margin-left:5px;margin-right:-5px;'>
									<span id='infotext'></span>
								</div>
							</div>
							"."
						" : "
							<div class='row'>
								<div class='col-sm-12'>
									<h4>It appears you haven't participated in a tournament yet.</h4>
								</div>
							</div>
						")."
					</div>
				</div>
				<div class='col-sm-12 col-md-7' >
					<div style='width: 100%; background-color: #ffffffb0; border-radius: 15px; padding: 5px;'>
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
									"."$paginator"."
								</tbody>
							</table>
						</div>
					</div>
				</div>
			</div>
			<div class='row'>
				<div style='padding-bottom: 50px;'></div>
			</div>
		";
	}
	else {
		$pagecontent = $notice."
			<div class='row'>
				<div class='col-sm-12' >
					<div style='width: 100%; background-color: #ffffffb0; border-radius: 15px; padding: 5px;'>
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
					</div>
				</div>
			</div>
			<div class='row'>
				<div style='padding-bottom: 50px;'></div>
			</div>
		";
	}

	$pagecontent .= "
		<script>
			function infoText(image) {
				document.getElementById(\"infotext\").innerHTML = \"Badge Title: \" + image.title;
			}

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

	include("./global.php");
?>