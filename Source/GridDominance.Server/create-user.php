<?php

require 'internals/backend.php';


function run() {
	global $pdo;

	$username      = GDUser::DEFAULT_USERNAME;
	$password      = getParamSHAOrError('password');
	$devicename    = getParamStrOrError('device_name');
	$deviceversion = getParamStrOrError('device_version');
	$appversion    = getParamStrOrError('app_version');

	$signature     = getParamStrOrError('msgk');

	check_commit_signature($signature, [$password, $appversion, $devicename, $deviceversion]);

	//---------

	$hash = password_hash($password, PASSWORD_BCRYPT);
	if (!$hash) throw new Exception('password_hash failure');

	$stmt = $pdo->prepare("INSERT INTO users(username, password_hash, is_auto_generated, score, creation_device_name, creation_device_version, last_online_app_version) VALUES (:un, :pw, 1, 0, :dn, :dv, :av)");
	$stmt->bindValue(':un', $username, PDO::PARAM_STR);
	$stmt->bindValue(':pw', $hash, PDO::PARAM_STR);
	$stmt->bindValue(':dn', $devicename, PDO::PARAM_STR);
	$stmt->bindValue(':dv', $deviceversion, PDO::PARAM_STR);
	$stmt->bindValue(':av', $appversion, PDO::PARAM_STR);
	executeOrFail($stmt);

	$user = GDUser::CreateNew($pdo->lastInsertId(), $username, 0);

	//---------

	logMessage("New user registered $user->ID");
	outputResultSuccess(['user' => $user]);
}



try {
	init("create-user");
	run();
} catch (Exception $e) {
	outputErrorException(Errors::INTERNAL_EXCEPTION, 'InternalError', $e, LOGLEVEL::ERROR);
}