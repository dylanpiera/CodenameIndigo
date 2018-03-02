<?php
	require_once("./preload.php");
	
	$provider = new \Discord\OAuth\Discord([
		'clientId' => '409423120194076683',
		'clientSecret' => 'jS28S7HLTv4aj61_eOqCZE3iFSrXyNmJ',
		'redirectUri' => 'http://jduriez.fr/bulbaleague/discord.php'
	]);

	$options['scope'] = ['identify'];
	
	$loginlink = "<a href='" . $provider->getAuthorizationUrl($options) . "'>Log in with Discord</a>";
	
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