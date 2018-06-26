<!DOCTYPE html>
<html prefix="og: http://ogp.me/ns#">
	<head>
		<meta charset="utf-8">
		<meta http-equiv="X-UA-Compatible" content="IE=edge">
		<meta name="viewport" content="width=device-width, initial-scale=1">

		<title>BulbaLeague - <?php echo($pagetitle); ?></title>

		<meta property="og:title" content="BulbaLeague - <?php echo($pagetitle); ?>" />
		<meta property="og:type" content="website" />
		<meta property="og:url" content="https://bulbaleague.soaringnetwork.com/<?php echo(strtolower($pagetitle));?>" />
		<meta property="og:image" content="https://cdn.bulbagarden.net/upload/1/1f/IRC_bulb.png" />
		<meta property="og:description" content="<?php echo($pagedescription);?>" />
		<meta name="theme-color" content="#d8fdff" />
		<link rel="icon" href="https://cdn.bulbagarden.net/upload/1/1f/IRC_bulb.png">
		
		<link rel="stylesheet"  type="text/css" href="css/newstyle.php">
		<link rel="stylesheet" href="css/sidebar-left.css">
		<link rel="stylesheet" href="https://use.fontawesome.com/releases/v5.0.13/css/all.css" integrity="sha384-DNOHZ68U8hZfKXOrtjWvjxusGo9WQnrNx2sqG0tfsghAvtVlRW3tvkXWZh58N9jp" crossorigin="anonymous">
		<link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.1.0/css/bootstrap.min.css" integrity="sha384-9gVQ4dYFwwWSjIDZnLEWnxCjeSWFphJiwGPXr1jddIhOegiu1FwO5qRGvFXOdJZ4" crossorigin="anonymous">
		<link href="https://fonts.googleapis.com/css?family=Ubuntu:400,700" rel="stylesheet">
	</head>
	<body>
		<?php include("./menu.php")?>

		<div class="main-content">
			<div class="header">
				<div class="headerlogo">
					<!--img src="https://cdn.discordapp.com/attachments/363753943555244032/439546197326430218/banner_for_DN.png"/>
					<img src="https://cdn.discordapp.com/attachments/363753943555244032/442415452778528781/BulbaLeague_Banner___Final.png"/-->
					<span style="text-align:center;display:block;height:300px;line-height: 300px;font-size: 4em;">Banner Space</span>
				</div>
			</div>
			<div class="container">
				<?php echo($pagecontent); ?>
			</div>
		</div>
		<script src="https://code.jquery.com/jquery-3.2.1.slim.min.js" integrity="sha384-KJ3o2DKtIkvYIK3UENzmM7KCkRr/rE9/Qpg6aAZGJwFDMVNA/GpGFF93hXpG5KkN" crossorigin="anonymous"></script>
		<script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.12.9/umd/popper.min.js" integrity="sha384-ApNbgh9B+Y1QKtv3Rn7W3mgPxhU9K/ScQsAP7hUibX39j7fakFPskvXusvfa0b4Q" crossorigin="anonymous"></script>
		<script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/js/bootstrap.min.js" integrity="sha384-JZR6Spejh4U02d8jOt6vLEHfe/JQGiRRSQQxSfFWpi1MquVdAyjUar5+76PVCmYl" crossorigin="anonymous"></script>
	</body>
</html>