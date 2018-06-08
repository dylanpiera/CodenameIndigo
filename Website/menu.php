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
		<?php if (isset($admin) || isset($skinedit)) {?><a class="link-black" href="./controlpanel.php"><i class="fa fa-exclamation-triangle"></i>Admin</a><?php } ?>
	</div>
</aside>