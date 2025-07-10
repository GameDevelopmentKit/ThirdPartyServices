mergeInto(LibraryManager.library, {
    AuthChangedWeb: function (cid) {
        var parsedCid = Pointer_stringify(cid);

        firebaseOnAuthStateChanged(function (user) {
            if (user) {
                unityInstance.Module.SendMessage("FirebaseWebGLListener", "CallbackHandler", JSON.stringify({"id": parsedCid, "status": true, "data": JSON.stringify(user)}));
            } else {
                unityInstance.Module.SendMessage("FirebaseWebGLListener", "CallbackHandler", JSON.stringify({"id": parsedCid, "status": true, "data": NULL}));
            }
        });
    }
    ,

    SignInWithEmailAndPasswordWeb: function (email, password, cid) {
        var parsedEmail = Pointer_stringify(email);
        var parsedPassword = Pointer_stringify(password);
        var parsedCid = Pointer_stringify(cid);

        try {
            firebaseAuthSignInEmailAndPassWord(firebaseAuth, parsedEmail, parsedPassword).then(function (userCredential) {
                unityInstance.Module.SendMessage("FirebaseWebGLListener", "CallbackHandler", JSON.stringify({"id": parsedCid, "status": true, "data": JSON.stringify(userCredential.user)}));
            }).catch(function (error) {
                unityInstance.Module.SendMessage("FirebaseWebGLListener", "CallbackHandler", JSON.stringify({
                    "id": parsedCid,
                    "status": false,
                    "data": JSON.stringify(error, Object.getOwnPropertyNames(error))
                }));
            });

        } catch (error) {
            unityInstance.Module.SendMessage("FirebaseWebGLListener", "CallbackHandler", JSON.stringify({
                "id": parsedCid,
                "status": false,
                "data": JSON.stringify(error, Object.getOwnPropertyNames(error))
            }));
        }
    }
    ,

    SignInWithCustomTokenWeb: function (token, cid) {
        var parsedToken = Pointer_stringify(token);
        var parsedCid = Pointer_stringify(cid);

        try {
            firebaseSignInWithCustomToken(firebaseAuth, parsedToken).then(function (userCredential) {
                unityInstance.Module.SendMessage("FirebaseWebGLListener", "CallbackHandler", JSON.stringify({"id": parsedCid, "status": true, "data": JSON.stringify(userCredential.user)}));
            }).catch(function (error) {
                unityInstance.Module.SendMessage("FirebaseWebGLListener", "CallbackHandler", JSON.stringify({
                    "id": parsedCid,
                    "status": false,
                    "data": JSON.stringify(error, Object.getOwnPropertyNames(error))
                }));
            });

        } catch (error) {
            unityInstance.Module.SendMessage("FirebaseWebGLListener", "CallbackHandler", JSON.stringify({
                "id": parsedCid,
                "status": false,
                "data": JSON.stringify(error, Object.getOwnPropertyNames(error))
            }));
        }
    }
    ,

    SignInWithGoogleWeb: function (cid) {
        var parsedCid = Pointer_stringify(cid);

        try {
            var provider = googleAuthProvider;
            firebaseSigninWithGoogle(firebaseAuth, provider).then(function (result) {
                unityInstance.Module.SendMessage("FirebaseWebGLListener", "CallbackHandler", JSON.stringify({"id": parsedCid, "status": true, "data": JSON.stringify(result.user)}));
            }).catch(function (error) {
                unityInstance.Module.SendMessage("FirebaseWebGLListener", "CallbackHandler", JSON.stringify({
                    "id": parsedCid,
                    "status": false,
                    "data": JSON.stringify(error, Object.getOwnPropertyNames(error))
                }));
            });

        } catch (error) {
            unityInstance.Module.SendMessage("FirebaseWebGLListener", "CallbackHandler", JSON.stringify({
                "id": parsedCid,
                "status": false,
                "data": JSON.stringify(error, Object.getOwnPropertyNames(error))
            }));
        }
    }
    ,

    SignInWithFacebookWeb: function (cid) {
        var parsedCid = Pointer_stringify(cid);

        try {
            var provider = new firebase.auth.FacebookAuthProvider();
            firebase.auth().signInWithRedirect(provider).then(function (result) {
                unityInstance.Module.SendMessage("FirebaseWebGLListener", "CallbackHandler", JSON.stringify({"id": parsedCid, "status": true, "data": JSON.stringify(result.user)}));
            }).catch(function (error) {
                unityInstance.Module.SendMessage("FirebaseWebGLListener", "CallbackHandler", JSON.stringify({
                    "id": parsedCid,
                    "status": false,
                    "data": JSON.stringify(error, Object.getOwnPropertyNames(error))
                }));
            });

        } catch (error) {
            unityInstance.Module.SendMessage("FirebaseWebGLListener", "CallbackHandler", JSON.stringify({
                "id": parsedCid,
                "status": false,
                "data": JSON.stringify(error, Object.getOwnPropertyNames(error))
            }));
        }
    }
    ,
    ResetPasswordWeb: function (email) {
           var emailAddress = Pointer_stringify(email);
        firebaseSendPasswordResetEmail(firebaseAuth,emailAddress)
            .then(function () {
                console.log('Password reset email sent');
        })
            .catch(function (error) {
                console.error(error.message);
            });        
    }
})
;
