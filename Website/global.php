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
		
		<script type="text/javascript" language="javascript" src="https://code.jquery.com/jquery-3.3.1.js"></script>
		<link rel="stylesheet"  type="text/css" href="css/newstyle.php">
		<link rel="stylesheet" href="css/sidebar-left.css">
		<link rel="stylesheet" href="https://use.fontawesome.com/releases/v5.0.13/css/all.css" integrity="sha384-DNOHZ68U8hZfKXOrtjWvjxusGo9WQnrNx2sqG0tfsghAvtVlRW3tvkXWZh58N9jp" crossorigin="anonymous">
		<link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.1.0/css/bootstrap.min.css" integrity="sha384-9gVQ4dYFwwWSjIDZnLEWnxCjeSWFphJiwGPXr1jddIhOegiu1FwO5qRGvFXOdJZ4" crossorigin="anonymous">
		<link href="https://fonts.googleapis.com/css?family=Ubuntu:400,700" rel="stylesheet">

		<!-- Date Time Picker -->
		<script src="https://npmcdn.com/tether@1.2.4/dist/js/tether.min.js"></script>
		<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/4.0.0-alpha.6/css/bootstrap.css" crossorigin="anonymous">
		<script src="https://cdnjs.cloudflare.com/ajax/libs/moment.js/2.17.0/moment-with-locales.js"></script>
		<script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/4.0.0-alpha.6/js/bootstrap.min.js"></script>
		<script src="https://rawgit.com/tempusdominus/bootstrap-4/master/build/js/tempusdominus-bootstrap-4.min.js"></script>
		<link href="https://rawgit.com/tempusdominus/bootstrap-4/master/build/css/tempusdominus-bootstrap-4.min.css" rel="stylesheet"/>

		<!-- Datatables  -->
		<link rel="stylesheet" type="text/css" href="https://cdn.datatables.net/1.10.19/css/dataTables.bootstrap4.min.css">
		<script type="text/javascript" language="javascript" src="https://cdn.datatables.net/1.10.19/js/jquery.dataTables.min.js"></script>
		<script type="text/javascript" language="javascript" src="https://cdn.datatables.net/1.10.19/js/dataTables.bootstrap4.min.js"></script>
		
		<!-- Matomo -->
		<script type="text/javascript">
		// require user consent before processing data
		_paq.push(['requireConsent']);
		_paq.push(['trackPageview']);

  		var _paq = _paq || [];
  		/* tracker methods like "setCustomDimension" should be called before "trackPageView" */
  		_paq.push(['trackPageView']);
  		_paq.push(['enableLinkTracking']);
  		(function() {
    		var u="//api.soaringnetwork.com/piwik/";
    		_paq.push(['setTrackerUrl', u+'piwik.php']);
    		_paq.push(['setSiteId', '1']);
    		var d=document, g=d.createElement('script'), s=d.getElementsByTagName('script')[0];
    		g.type='text/javascript'; g.async=true; g.defer=true; g.src=u+'piwik.js'; s.parentNode.insertBefore(g,s);
  		})();
		</script>
		<!-- End Matomo Code -->

		<!-- Bootstrap JS -->
		<script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.3/umd/popper.min.js" integrity="sha384-ZMP7rVo3mIykV+2+9J3UJ46jBk0WLaUAdn689aCwoqbBJiSnjAK/l8WvCWPIPm49" crossorigin="anonymous"></script>
		<script src="https://stackpath.bootstrapcdn.com/bootstrap/4.1.3/js/bootstrap.min.js" integrity="sha384-ChfqqxuZUCnJSK3+MXmPNIyE6ZbWh2IMqE241rYiqJxyMiZ6OW/JmZQ5stwEULTy" crossorigin="anonymous"></script>
	</head>
	<body>
		<?php include("./menu.php")?>

		<div class="main-content">
			<div class="header">
				<div class="headerlogo" style='background-image: url("/img/banner_pt_2.png"); background-position: left center; background-size:cover; background-repeat: no-repeat; height: 300px; text-align:right;'>
        			<div class="row justify-content-end" style="max-height: 300px; height: 100%; max-width:100%">
        				<div class="col-md-6" align="center" style="padding: 0;">
        					<span style="display: inline-block; height: 100%; vertical-align: middle;"></span>
							<img src='/img/logo.png' alt='Bulbaleague Logo!' style='max-height: 300px; background-color: transparent;'>
        				</div>
       				</div>
					<!--span style="text-align:center;display:block;height:300px;line-height: 300px;font-size: 3em;">Banner Space</span-->
				</div>
			</div>
			<div class="container">
				<?php echo($pagecontent); ?>
			</div>
		</div>
	</body>
</html>
