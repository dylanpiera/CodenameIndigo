<!DOCTYPE html>
<html>
	<head>
		
		<meta charset="utf-8">
		<meta http-equiv="X-UA-Compatible" content="IE=edge">
		<meta name="viewport" content="width=device-width, initial-scale=1">

		<title>BulbaLeague - <?php echo($pagetitle); ?></title>
		
		<link rel="stylesheet"  type="text/css" href="css/newstyle.php">
		<link rel="stylesheet" href="css/sidebar-left.css">
		<link rel="stylesheet" href="https://use.fontawesome.com/releases/v5.0.13/css/all.css" integrity="sha384-DNOHZ68U8hZfKXOrtjWvjxusGo9WQnrNx2sqG0tfsghAvtVlRW3tvkXWZh58N9jp" crossorigin="anonymous">
		<link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.1.0/css/bootstrap.min.css" integrity="sha384-9gVQ4dYFwwWSjIDZnLEWnxCjeSWFphJiwGPXr1jddIhOegiu1FwO5qRGvFXOdJZ4" crossorigin="anonymous">
		<link href='http://fonts.googleapis.com/css?family=Cookie' rel='stylesheet' type='text/css'>
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
	</body>
</html>