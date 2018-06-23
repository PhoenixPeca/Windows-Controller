<?php
set_time_limit(false);
$login_auth = 'Authorization:Digest username="admin", realm="Highwmg", nonce="26353868", uri="/cgi/xml_action.cgi", response="1c079476508a1af131edb8b41265f908", qop=auth, nc=00000003, cnonce="f42ae252db17798a"';
$send_auth = 'Authorization:Digest username="admin", realm="Highwmg", nonce="26353868", uri="/cgi/xml_action.cgi", response="6a3260a3a45d2ec2fe6a65a851fb6455", qop=auth, nc=0000000C, cnonce="71c0055ad3ffd6f4"';
$smsdel_auth = 'Authorization:Digest username="admin", realm="Highwmg", nonce="26466762", uri="/cgi/xml_action.cgi", response="b408fd0f43d6ed4f73d39e25d705c2d6", qop=auth, nc=00000010, cnonce="be5fa9da25da871c"';

$trim = true;

echo '<form action="send.php" method="POST">Number: <input type="text" name="number"><br>Message: <input type="text" name="message"><br><input type="submit" value="Submit"></form>';

if(empty($_POST['number']) && empty($_POST['message'])) {
	exit;
} elseif(empty($_POST['number']) || empty($_POST['message'])) {
	die('Input must not be empty');
}

if($trim) {
	$_POST['message'] = substr($_POST['message'], 0, 764);
}

if(strlen($_POST['message']) > 765) {
	die('Message must not be more than 765 characters.');
}

$number = $_POST['number'];
$message = $_POST['message'];

$ch = curl_init();
curl_setopt($ch, CURLOPT_URL, "http://192.168.8.1/login.cgi");
curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
curl_setopt($ch, CURLOPT_CUSTOMREQUEST, "GET");
$headers[] = $login_auth;
curl_setopt($ch, CURLOPT_HTTPHEADER, $headers);
$result = curl_exec($ch);
curl_close ($ch); unset($ch, $headers);

if(!preg_match("/^(HTTP\/1\.1 200 OK)/", $result)) {
	die('Auth string for `login` is invalid.');
}

$ch = curl_init();
curl_setopt($ch, CURLOPT_URL, "http://192.168.8.1/xml_action.cgi?method=set&module=duster&file=message");
curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
curl_setopt($ch, CURLOPT_POSTFIELDS, message_compose($number, $message));
curl_setopt($ch, CURLOPT_POST, 1);
$headers[] = $send_auth;
curl_setopt($ch, CURLOPT_HTTPHEADER, $headers);
$result = curl_exec($ch);
curl_close ($ch); unset($ch, $headers);

if(empty($result) || !preg_match('/<sms_cmd_status_result>(.*?)<\/sms_cmd_status_result>/si', $result, $msg_sta) || !preg_match('/<sms_nv_send_num>(.*?)<\/sms_nv_send_num>/si', $result, $msg_id)) {
	die('Auth string for `sms_send` is invalid.');
}

$msg_sta = $msg_sta[1];
$msg_id = $msg_id[1];

if(intval($msg_sta) !== 3) {
	echo "Message not sent.\n";
} else {
	echo "Message sent!\n";
}

$ch = curl_init();
curl_setopt($ch, CURLOPT_URL, "http://192.168.8.1/xml_action.cgi?method=set&module=duster&file=message");
curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
curl_setopt($ch, CURLOPT_POSTFIELDS, "<?xml version=\"1.0\" encoding=\"US-ASCII\"?> <RGW><message><flag><message_flag>DELETE_SMS</message_flag><sms_cmd>6</sms_cmd></flag><get_message><tags>2</tags><mem_store>1</mem_store></get_message><set_message><delete_message_id>LSNT$msg_id,</delete_message_id></set_message></message></RGW>");
curl_setopt($ch, CURLOPT_POST, 1);
$headers[] = $smsdel_auth;
curl_setopt($ch, CURLOPT_HTTPHEADER, $headers);
$result = curl_exec($ch);
curl_close ($ch); unset($ch, $headers);

if(empty($result) || !preg_match('/<sms_cmd_status_result>(.*?)<\/sms_cmd_status_result>/si', $result, $msg_sta) || !preg_match('/<sms_nv_send_num>(.*?)<\/sms_nv_send_num>/si', $result, $msg_id)) {
	die('Auth string for `sms_delete` is invalid.');
}

$msg_sta = $msg_sta[1];
$msg_id = $msg_id[1];

if(intval($msg_sta) !== 3) {
	echo "Message deletion failed.\n";
}

function message_compose($number, $string) {
	$encoded_msg = substr(unpack('H*', iconv("UTF-8", "UTF-16", $string))[1], 4);
	return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>
	<RGW>
	   <message>
		  <flag>
			 <message_flag>SEND_SMS</message_flag>
			 <sms_cmd>4</sms_cmd>
		  </flag>
		  <send_save_message>
			 <contacts>$number</contacts>
			 <content>$encoded_msg</content>
			 <encode_type>GSM7_default</encode_type>
			 <sms_time>".date('y,n,j,g,').intval(date('i')).','.intval(date('s')).','.date('O')."</sms_time>
		  </send_save_message>
	   </message>
	</RGW>";
}