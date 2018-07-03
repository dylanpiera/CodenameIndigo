<?php
header("Content-type: text/css; charset: UTF-8");
chdir("../");
require_once("./preload.php");

$background_color = getColorFromDB("mbgcolor", $db, $uid);
$header_color = getColorFromDB("hbgcolor", $db, $uid);
$main_color = getColorFromDB("mccolor", $db, $uid);
$sidebar_color = getColorFromDB("sbcolor", $db, $uid);
$sidebar_bg_color = getColorFromDB("smbgcolor", $db, $uid);
$sidebar_sbg_color = getColorFromDB("ssbgcolor", $db, $uid);
?>

html, body {
    height: 100%;
}

.sidebar-left {
    background-color: <?php echo($sidebar_bg_color);?> !important;
}

.tournament {
    background-color: <?php echo($header_color."90");?>;
    padding: 10px;
    border-radius: 15px;
}

.sidebar-left button {
    border-color: <?php echo($sidebar_sbg_color);?> !important;
    color: <?php echo($sidebar_color);?> !important;
}

.sidebar-left button:hover {
    background-color: <?php echo($sidebar_sbg_color);?> !important;
}

.sidebar-left .sidebar-links a {
    background-color: <?php echo($sidebar_sbg_color);?> !important;
    color: <?php echo($sidebar_color);?> !important;
}

.headerlogo {
    background-color: <?php echo($header_color);?> !important;
}

.main-content, body {
    background-color: <?php echo($background_color);?> !important;
    color: <?php echo($main_color);?> !important;
}


