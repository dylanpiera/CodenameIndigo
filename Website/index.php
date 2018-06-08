<?php
	require_once("./preload.php");
	
	$pagetitle = "Home";
	$pagecontent = "
		<div class='row'>
			<div class='col'>
			<h1>Welcome to Bulbaleague!</h1>
			Bulbaleague is Bulbagarden's personal Pokemon League, with our own Elite Four and Champion. Every so often we hold a tournament to determine which lucky few get to challenge our E4 and, if they beat the E4, to challenge the Champ. Those who manage to defeat our League can then take one of the Elite Four or Champion titles for themselves, depending on how well they did against the current E4/Champ. They will then be displayed in our <a href='./halloffame.php'>Hall of Fame</a> for all to see.<br>
			<br>
			Our tournament is fought in <a href='https://play.pokemonshowdown.com'>Pokémon Showdown</a> using Gen 7 (US/UM singles) OU tier, and the tourney is discussed on <a href='https://discord.gg/bulbagarden'>Bulbagarden's Discord server</a> in the #bulbaleague channel. We use the replays feature from Showdown to confirm the winners of each matchup. Some of these replays will even be displayed on <a href='https://www.youtube.com/user/Bulbagarden'>our YouTube channel</a>.<br>
			<br>
			<span class='smalltext'>Special thanks goes to: Saphir and SoaringDylan for all their hard work on the website and the bot and all the jokes on hard days, abcboy for his endless ideas, wisdom, and problem-solving, user MariokartDJ on Discord for giving me the seed of an idea that became this tournament, and Pyrax, noworry, Cresselia92, Lone_Garurumon, Water Max, Rocket Queen, Soki, Slife, Erza Scarlett, Maniacal Engineer, dig, Lucy, Doggo, Zexy, DarthWolf, Rhodehawk, and TeaWest for their endless battling in developing this tourney.</span>
			</div>
		</div>
	";
	
	include("./global.php");
?>