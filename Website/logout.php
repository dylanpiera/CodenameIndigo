<?php
require_once('./preload.php');

$pagetitle = "Logout";
$pagedescription = "Logout from the website.";

unset($_SESSION['user']);
session_destroy();

if(isset($_SESSION['page'])) {
	header("Location: /".$_SESSION['page']);
} else {
	header("Location: ./index.php");
}
?>