<?php
	require_once("./preload.php");
	
	$tournamentlist = "";
	$tournament_q = $db->query("SELECT * FROM tournaments WHERE closure >= " . time() . " ORDER BY regstart DESC");
	while($tournament = $tournament_q->fetch()) {
		if(time() <= $tournament['regend']) {
			$tournamentlist .= "
				<div class='tournament'>
					<strong>" . $tournament['tournament'] . "</strong><br>
					<em>Registrations for this tournament have not ended yet.</em>
				</div>
			";
		}
		else {
			$player = array();
			$tdcount = array();
			$round = array();
			$participant_q = $db->query("SELECT * FROM participants WHERE tid = " . $tournament['tid']);
			while($participant = $participant_q->fetch()) {
				$player[$participant['pid']] = $participant;
			}
			$player[0] = array(
				'discordusername' => "/ Bye /",
				'showdownusername' => "Randomly selected to automatically advance to the next round."
			);
			$battle_q = $db->query("SELECT * FROM battles WHERE tid = " . $tournament['tid'] . " ORDER BY bid DESC");
			while($battle = $battle_q->fetch()) {
				if(!isset($tdcount[$battle['round']])) {
					$tdcount[$battle['round']] = 0;
					$round[$battle['round']] = "";
				}
				if($tdcount[$battle['round']] == 0) {
					$round[$battle['round']] .= "<tr>";
				}
				switch($battle['winner']) {
					case 1:
						$p1class = "green";
						$p2class = "red";
						break;
					case 2:
						$p1class = "red";
						$p2class = "green";
						break;
					default:
						$p1class = "";
						$p2class = "";
						break;
				}
				$round[$battle['round']] .= "
				<div class='col-sm-12 col-md-4 col-lg-3'>
					<table class='table battle'>
						<tbody>
							<tr>
								<td class='" . $p1class . "' title=\"" . esc($player[$battle['player1']]['showdownusername']) . "\">" . esc($player[$battle['player1']]['discordusername']) . "</td>
							</tr>
							<tr>
								<td><span class='font-weight-bold'>Versus</span></td>
							</tr>
							<tr>
								<td class='" . $p2class . "' title=\"" . esc($player[$battle['player2']]['showdownusername']) . "\">" . esc($player[$battle['player2']]['discordusername']) . "</td>
							</tr>
						</tbody>
					</table>
				</div>
				";
				$tdcount[$battle['round']]++;
				if($tdcount[$battle['round']] == 6) {
					$round[$battle['round']] .= "</tr>";
					$tdcount[$battle['round']] = 0;
				}
			}
			ksort($round);
			$tournamentlist .= "
			<div class='row'>
				<div class='col-sm-12'>
					<div class='tournament'>
						<strong>" . $tournament['tournament'] . "</strong><br>
			";
			foreach($round as $roundnum => $roundbrackets) {
				$tournamentlist .= "<hr>
						<em>Round #" . $roundnum . "</em><br>
						<div class='row justify-content-around' style='margin: 0;'>
							" . $roundbrackets . "
						</div>
							";
						}
			$tournamentlist .="
					</div>
				</div>";
		}
	}
	if(!isset($tournamentlist)) {
		$tournamentlist = "There is no tournament running nor planned at the moment.";
	}
		
	$pagetitle = "Brackets";
	$pagecontent = "
		" . $tournamentlist . "
	";
	$pagedescription = "The current standings are...";
	
	include("./global.php");
?>