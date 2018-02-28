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
			<span class='loginlink'>" . $loginlink . "</span>
		";
	}
?>

<div class="discord">
	<div class="avatar" style="background-image: url('<?php echo($avatar); ?>');"></div>
	<div class='accountdetails'>
		<?php echo($accdetails); ?>
	</div>
</div>
<div class="menuitems">
	<div class="menuitem"><a href="./index.php">Home</a></div>
	<div class="menuitem"><a href="./rules.php">Rules</a></div>
	<?php if(isset($uid)) { ?><div class="menuitem"><a href="./register.php">Register</a></div><?php } ?>
	<div class="menuitem"><a href="./brackets.php">Brackets</a></div>
	<div class="menuitem"><a href="./planning.php">Planning</a></div>
	<div class="menuitem"><a href="./halloffame.php">Hall of Fame</a></div>
	<?php if(isset($admin)) { ?><div class="menuitem"><a href="./controlpanel.php">Control Panel</a></div><?php } ?>
</div>