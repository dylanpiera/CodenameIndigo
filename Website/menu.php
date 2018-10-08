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

<div class="collapseButton">
	<button style="padding: 4px 8px;" type="button" class="btn btn-info" data-toggle="collapse" data-target="#sidebarCollapse" aria-expanded="true" aria-controls="#sidebarCollapse">
		<div onclick='this.classList.toggle("change");'>
			<div class="bar1"></div>
			<div class="bar2"></div>
			<div class="bar3"></div>
		</div>
	</button>
</div>
<aside class="sidebar-left collapse" id="sidebarCollapse">
	<div class="sidebar-links">
		<span class="discord">
			<span class="accountdetails">
				<span class="loginlink">
					<a class="link-cyan" onClick="discordLogin('<?php if(!isset($uid)) echo($loginlink); else echo("./logout"); ?>')"  href="#">
					<i class="fa"><img class="logoimg" src="<?php echo($avatar); ?>"/> </i>
						<?php echo($accdetails); ?>
				</a></span>
			</span>
		</span>
		<a class="link-blue" href="./index"><i class="fa fa-home"></i>Home</a>
		<a class="link-red" href="./rules"><i class="fa fa-bolt"></i>Rules</a>
		<?php if (isset($uid)) {?><a class="link-orange" href="./register"><i class="fa fa-sign-in-alt"></i>Register</a><?php } ?>
		<a class="link-yellow" href="./brackets"><i class="fa fa-balance-scale"></i>Brackets</a>
		<a class="link-green" href="./members"><i class="fa fa-address-book"></i>Members</a>
		<a class="link-gray" href="./halloffame"><i class="fa fa-flag"></i>Hall of Fame</a>
        <a class="link-white" href="./contact"><i class="fa fa-comment"></i>Contact Us!</a>
		<?php if (isset($admin) || isset($skinedit)) {?><a class="link-black" href="./controlpanel"><i class="fa fa-exclamation-triangle"></i>Admin</a><?php } ?>
	</div>
    <script>
        function discordLogin(link) {
        	<?php $_SESSION['page'] = basename($_SERVER['PHP_SELF']);; ?>
        	window.location.href = link;
        }
    </script>
</aside>