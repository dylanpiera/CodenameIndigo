<?php
	require_once("./preload.php");
	
	$pagetitle = "Rules";
	$pagecontent = "
		<div class='row'>
			<div class='col-sm-12'>
				<div class='card' style='margin-bottom: 40px;'>
					<div class='card-header' id='headerTarget'>
						<h5 class='mb-0'>
							<button class= 'btn btn-link' data-toggle='collapse' data-target='#collapseTarget' aria-expanded='true' aria-control='collapseTarget'>
								Staff For Fun - Short Rules
							</button>
						</h5>
					</div>
					<div id='collapseTarget' class='collapse show' aria-labelledby='headerTarget'>
						<div class='card-body'>
							<h1>
							Bulbaleague Staff Tourney
							</h1>
							<p style='margin-bottom: 20px;'>
							Welcome fellow staffer! During the bulbaholidays a lot of fun events will be organized. The Bulbaleague will be one of them!<br>
							<div class='alert alert-warning' role='alert'>
								<h5 class='alert-heading font-italic'>Please Note</h5><hr>
								<span class='mb-0'>
								While this tourney is for fun, feel free to report any bugs or problems you encounter while using either the website or the bot! We will add any issue to the to-do list and fix them up later!<br>
								<small class='font-italic'>The best way of doing so is by PMing Dylan</small>
								</span>
							</div>
							</p>
							<hr>
							<h3>
							How to join
							</h3>
							<p>
							To join you can use either the bot or the website.<br><br>

							The tournament is in <u>single elimination</u> format and will be run on <a href='https://pokemonshowdown.com'>Pokemon Showdown</a><br>
							<small class='text-muted'>
							This means that each person is paired with an opponent, where the winner of the battle will continue to the next round.<br><br>
							</small>
							<hr>
							<h3>The Format</h3>
							<ul>
								<li>OU tier</li>
								<li>timer off for battles.</li>
								<li>Rematch if battle ends in a draw.</li>
							</ul>
							<small class='text-muted'>
								Please make sure that your team is OU legal before registering with it. <u>See info below. \"How to use Pokemon Showdown\"</u>
							</small>
							<br>
							<p>
								The team you sign up with stays hidden for others until the tournament starts. After which you can't change your team anymore.<br>
								The first round lasts 1 week and a half. Starting on Sunday and ending on Wednesday the week after. <br>
								Any subsequential round will last 1 week, from Wednesday to Wednesday.<br>
							</p>
							<hr>
							<h3>How to Play</h3>
							<p>
								At the beginning of every round, the matchups will be made public. <br>
								You will have to contact your partner to organise a time to battle. 
								<br><br>
								After your battle both players must upload the replay on discord and mention DN and Dylan, and specify who won/lost. 
							</p>
							<br>
							<div id='showdownHeader'>
								<a data-toggle='collapse' href='#showdownCollapse' role='button' aria-expanded='false' aria-control='showdownCollapse'>
									<h5>How to use Pokemon Showdown</h5>
								</a>
							</div>
							<div id='showdownCollapse' class='collapse' aria-labelledby='showdownHeader'>
								<div class='card'>
									<div class='card-body'>
										<small class='text-muted'>This section is for those who haven't used Showdown Before</small>
										<br><br>
										<span id='menu'><h5 class='mt-0'>Main Page</h5></span>
										<img src='http://bulbaimages.kaoticsilence.com/howto1.jpg' class='img-fluid' alt='Showdown Homepage'>
										<br>
										This is the main page of Showdown. The top-right red arrow points to \"Choose Name,\" where you can choose your username. After you choose a name, \"Choose Name\" will be replaced with your username. Now you can click the gear icon to the right of your new username and use Register to keep your new username and add a password to your account. The arrows on the right side of the screen point you to Teambuilder.
										<br><br>
										<span id='team'><h5 class='mt-0'>Team Builder</h5></span>
										In the next screen choose New Team. From there you can add pokemon until you fill your party. You can choose your stats for them, held item, ability, etc.
										<br>
										<img src='http://bulbaimages.kaoticsilence.com/howto2.jpg' class='img-fluid' alt='Showdown Teambuilder'>
										<br>
										The second arrow points to \"Validate,\" which will tell you if your team fits into the tier we're using (Gen 7 OU), and if it doesn't fit into that tier it will tell you what you need to fix in order for it to fit. From here you can click on the different sections for each of your Pokemon to edit stuff (for instance, if you want to change the moves just click on the moves in that Pokemon's section).
										<br><br>
										The first arrow points to \"Import/Export.\" This is the button you need to click to register your team with the bot or here on the website. It will take you to a text list of your team's stuff, which you can then copy-paste when you use ?register for the bot or in the appropriate input field when registering on here. 
										<br><br>
										<span id='replay'><h5 class='mt-0'>Replays & How to Save Them</h5></span>
										<img src='http://bulbaimages.kaoticsilence.com/howto3.jpg' class='img-fluid' alt='Showdown Upload Replay'>
										<br>
										After your battle, this \"Upload and share replay\" button will appear. Click it to get the link you need to pm me or post it in the thread on the forums. I think that's it! Good luck and have fun, you guys, and thanks so much for participating!
									</div>
								</div>
							</div>
						</div>
					</div>
				</div>
			</div>
		</div>
		<div class='row'>
			<div class='col-sm-12'>
				<h1>
				Bulbaleague Rules & Guidelines
				</h1>
				<div class='alert alert-warning'>Please note that these rules are just a mockup and are subject to change. If you're here for the staff tourney please refer to the rules above.</div>
				<p>
				<b>Registration is via the bot or on the website</b>

				<ul>
					<li>OU tier</li>
					<li>timer off for battles.</li>
					<li>Rematch if battle ends in a draw.</li>
				</ul>

				<b>Must register teams a week in advance</b> so we can check if they are OU. Registration should open two weeks before tourney starts to give us enough time to check teams.<br>
				Matchups are posted the day the tourney starts (Sunday) and people have a week and a half (following Wednesday) to do their round 1 battles.  Subsequent rounds will be one week long.<br>  
				<i>Opponents' teams are not visible until after registration closes for fairness.</i>  
				<br><br>
				One team per person, and cannot change teams during the same tourney.  Can change teams between tournaments (but not between rounds of the same tournament).
				<br><br>
				Forfeits must be done by pm'ing dragon_nataku#7446, Saphir#0001, or SoaringDylan#0380. This will be an auto-win for the opponent.  If your partner fails to show up please send us a screenshot of your pm's proving you 
				attempted to arrange a time to battle, and you'll be declared the winner.  If both opponents fail to battle then both are disqualified.
				<br><br>
				Showdown replay must be submitted by BOTH participants to ensure validity.  Failure to submit your replay will lead to disqualification.
				<br><br>
				Odd number of participants means someone will get a bye (advance to next round without battling).  This will be randomly determined by the bot.
				</p>
				<h3>				
				Format: 
				</h3>
				<p>
				<b>Regular tourney format (single elimination).</b><br> 
				Each person is paired with an opponent, and losing opponent gets eliminated from the tourney. <br>
				This continues until there are between three and four winners.  (Victory Road)
				<br><br>
				These three or four people will have one week to fight all four Elite Four members and win.  Any losses in this round will disqualify them from fighting the Champion.  
				</p>
            </div>
		</div>
	";
	$pagedescription = "Registration is via the bot or on the website...";
	
	include("./global.php");
?>