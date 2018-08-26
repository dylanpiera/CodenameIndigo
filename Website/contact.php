<?php
	require_once("./preload.php");
	
	if(isset($uid)) {
    	if(CheckInServer($uid)) {
        	$pagecontent = "Welcome member!";
        }
    	else {
        	$pagecontent = "Only server members can use this. <a>https://discord.gg/bulbagarden</a>";
        }
    }
	else {
		$pagecontent = "
			<div class='row'>
				<div class='col-sm-12'>
					Coming Soon.
				</div>
			</div>
		";
    }

	if(!isset($uid)) {
    	$pagetitle = "Contact Us!";
		$pagecontent = "
			<div class='row'>
				<div class='col-sm-12'>
					Have any questions? Let us know via this form and we will answer your question on Discord as soon as possible!
				</div>
			</div>
            <hr>
            <div class='row'>
            	<div class='col-sm-12'>
                	<b><h4>You need to be logged in to use this page.</h4></b>
                    <p><a href='https://discordapp.com/api/oauth2/authorize?scope=identify&state=LgTIqzHaSjjdcfrIbC94VPgOqFMfw0u3&response_type=code&approval_prompt=auto&client_id=409423120194076683&redirect_uri=https%3A%2F%2Fbulbaleague.soaringnetwork.com%2Fdiscord'>Login Now</a</p>
                </div>
            </div>
		";
	}
	else {
		$pagetitle = "Contact Us!";
		$pagecontent = "
			<div class='row'>
				<div class='col-sm-12'>
					Have any questions? Let us know via this form and we will answer your question on Discord as soon as possible!
				</div>
			</div>
            <hr>
            ".((CheckInServer($uid)) ? "
            <div class='row'>
				<div class='col-sm-12'>
					<form action='contact.php' method='post'>
                    	<div class='form-group'>
                        	<div class='form-row'>
                            	<div class='col-sm-12 col-md-3'>
                                	<div class='form-row'>
                                    	<div class='col-sm-12'>
                                			<label for='username'>Discord Username</label>
                                			<input name='username' id='username' type='text' class='form-control' value='".$username."#".$discriminator."' disabled>
                                      	</div>
                                	</div>
                                    <div style='padding: 10px 0;'></div>
                                    <div class='form-row'>
                                    	<div class='col-sm-6'>
                                			<button type='submit' class='btn btn-primary'>Send Message</button>
                                      	</div>
                                        <div class='col-sm-6'>
                                			".(isset($_POST['message']) ? (SendMessageToChannel($_POST['message'], /*409419573322973194 /**/ 482152898009104384 /* #bulbaleaguestaff*/, [ 'username' => $username.'#'.$discriminator, 'avatar' => $avatar]) ? "
                                            	<div class='alert alert-success' role='alert'>
													Message sent successfully!
												</div>" : "
                                                <div class='alert alert-danger' role='alert'>
													An error occured. Please try to contact SoaringDylan#0380 on Discord.
												</div>
                                                ") : "")."
                                      	</div>
                                	</div>
                                </div>
                                <div class='col-sm-12 col-md-9'>
                                	<label for='message'>Your Message</label>
                                	<textarea class='form-control' id='message' name='message' style='height: 300px;' required>".((isset($_POST['message']) ? htmlspecialchars(addslashes($_POST['message'])) : "" ))."</textarea>
                                    <small class='form-text text-muted'>Please upload any image attachments to an image host (like imgur) and link to them in your message.</small>
                                </div>
                            </div>
                        </div>
                    </form>
				</div>
			</div>
            " : "
            <div class='row'>
            	<div class='col-sm-12'>
                	<b><h4>You need to be a member of Bulbagarden to use this page.</h4></b>
                    <p><a href='https://discord.gg/bulbagarden'>Join Bulbagarden Now!</a</p>
                </div>
            </div>
            ")."
		";
    }

	//SendWebhook(array ( 'discordusername' => $username.$discriminator, 'avatar' => $avatar), "A random message");
	$pagedescription = $pagecontent;

	include("./global.php");
?>