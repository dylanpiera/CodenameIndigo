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
	<div class="collapseButton">
		<button style="font-size: 1.5em;" type="button" class="btn btn-outline-info" data-toggle="collapse" data-target="#sidebarCollapse" aria-expanded="true" aria-controls="#sidebarCollapse">><!--i class="fas fa-bars"></i--></button>
	</div>
	<div class="collapse show" id="sidebarCollapse">
		<div class="sidebar-links">
			<span class="discord">
				<span class="accountdetails">
					<span class="loginlink">
						<a class="link-cyan" href="<?php if(!isset($uid)) echo($loginlink); else echo("./logout") ?>">
						<i class="fa"><img class="logoimg" src="<?php echo($avatar); ?>"/> </i>
							<?php echo($accdetails); ?>
					</a></span>
				</span>
			</span>
			<a class="link-blue" href="./index"><i class="fa fa-home"></i>Home</a>
			<a class="link-red" href="./rules"><i class="fa fa-bolt"></i>Rules</a>
			<?php if (isset($uid)) {?><a class="link-orange" href="./register"><i class="fa fa-sign-in-alt"></i>Register</a><?php } ?>
			<a class="link-yellow" href="./brackets"><i class="fa fa-balance-scale"></i>Brackets</a>
			<a class="link-green" href="./planning"><i class="fa fa-bookmark"></i>Planning</a>
			<a class="link-gray" href="./halloffame"><i class="fa fa-flag"></i>Hall of Fame</a>
			<?php if (isset($admin) || isset($skinedit)) {?><a class="link-black" href="./controlpanel"><i class="fa fa-exclamation-triangle"></i>Admin</a><?php } ?>
		</div>
	</div>
</aside>