<?php
require_once('./preload.php');

$pagetitle = "Logout";
$pagedescription = "Logout from the website.";

unset($_SESSION['user']);
session_destroy();

header("Location: ./index.php");
?>