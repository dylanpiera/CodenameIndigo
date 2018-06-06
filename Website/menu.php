<?php
	if(isset($uid)) {
		$accdetails = "
			<span class='username'>" . $username . "</span><br>
			<span class='discriminator'>#" . $discriminator . "</span>
		";
	}
	else {
		$avatar = "./defaultavatar.png";
		$accdetails = "
			Login with Discord
		";
	}
?>

<aside class="sidebar-left">
	<div class="sidebar-links">
		<span class="discord">
			<span class="accountdetails">
				<span class="loginlink">
					<a class="link-cyan" href="<?php if(!isset($uid)) echo($loginlink); else echo("./logout.php") ?>">
					<i class="fa"><img class="logoimg" src="<?php echo($avatar); ?>"/> </i>
						<?php echo($accdetails); ?>
				</a></span>
			</span>
		</span>
		<a class="link-blue" href="./index.php"><i class="fa fa-home"></i>Home</a>
		<a class="link-red" href="./rules.php"><i class="fa fa-bolt"></i>Rules</a>
		<?php if (isset($uid)) {?><a class="link-orange" href="./register.php"><i class="fa fa-sign-in-alt"></i>Register</a><?php } ?>
		<a class="link-yellow" href="./brackets.php"><i class="fa fa-balance-scale"></i>Brackets</a>
		<a class="link-green" href="./planning.php"><i class="fa fa-bookmark"></i>Planning</a>
		<a class="link-gray" href="./halloffame.php"><i class="fa fa-flag"></i>Hall of Fame</a>
		<?php if (isset($admin)) {?><a class="link-black" href="./controlpanel.php"><i class="fa fa-exclamation-triangle"></i>Admin</a><?php } ?>
	</div>
</aside>

<!--div class="discord">
	<div class="avatar" style="background-image: url('<?php //echo($avatar); ?>');"></div>
	<div class='accountdetails'>
		<?php //echo($accdetails); ?>
	</div>
</div>
<div class="menuitems">
	<div class="menuitem"><a href="./index.php">Home</a></div>
	<div class="menuitem"><a href="./rules.php">Rules</a></div>
	<?//php if(isset($uid)) { ?><div class="menuitem"><a href="./register.php">Register</a></div><?//php } ?>
	<div class="menuitem"><a href="./brackets.php">Brackets</a></div>
	<div class="menuitem"><a href="./planning.php">Planning</a></div>
	<div class="menuitem"><a href="./halloffame.php">Hall of Fame</a></div>
	<?//php if(isset($admin)) { ?><div class="menuitem"><a href="./controlpanel.php">Control Panel</a></div><?//php } ?>
</div-->