<?php
	require_once("./preload.php");
	
	$provider = new \Discord\OAuth\Discord([
		'clientId' => '409423120194076683',
		'clientSecret' => 'qmX2Hxo6DAANJ2_6Nb6cvG2XX8dUi3R7',
		'redirectUri' => 'https://bulbaleague.soaringnetwork.com/discord.php'
	]);

	$options['scope'] = ['identify'];
	
	$loginlink = "" . $provider->getAuthorizationUrl($options);
	
	if(isset($_GET['code'])) {
		$token = $provider->getAccessToken('authorization_code', ['code' => $_GET['code']]);
		$user = $provider->getResourceOwner($token)->toArray();
		$_SESSION['user'] = $user;
		$success = true;
	}
	
	if(isset($success)) {
		header("Location: ./index.php");
	}
?>