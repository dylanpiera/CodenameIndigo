<!DOCTYPE html>
<html>
	<head>
		<title>BulbaLeague - <?php echo($pagetitle); ?></title>
		<link rel="stylesheet" href="style.css">
		<meta charset="UTF-8">
	</head>
	<body>
		<div class="title">
			<img src="title.png" alt="BulbaLeague">
		</div>
		<table class="main">
			<tr>
				<td class="menu">
					<?php include("./menu.php"); ?>
				</td>
				<td class="content">
					<div class="contenttitle"><?php echo($pagetitle); ?></div>
					<?php echo($pagecontent); ?>
				</td>
			</tr>
		</table>
	</body>
</html>