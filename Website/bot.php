<?php
	require_once("./preload.php");
	
	if(isset($_POST['select'])) {
		$select = $_POST['select'];
	}


	$pagetitle = "Bot";
	$pagecontent = "
		<div class='row'>
			<div class='col-sm-12'>
			". ((isset($select) == true) ? "Hello!" : "" )."
				<form action='bot.php' method='post'>
					<select name='select'>
						<option value='msg'>Send Message</option>
					</select>
					<input type='submit'>
				</form>
			</div>
		</div>
	";



	//SendWebhook(array ( 'discordusername' => $username.$discriminator, 'avatar' => $avatar), "A random message");
	$pagedescription = $pagecontent;

	include("./global.php");
?>