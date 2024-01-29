// Get the modals
var loginModal = document.getElementById("loginModal");
var signupModal = document.getElementById("signupModal");
var forgotModal = document.getElementById("forgotPassword");

// Get the buttons that open the modals
var loginBtn = document.getElementById("loginLink");
var signupBtn = document.getElementById("signUpLink");

// Get the buttons that open the modals
var forgotBtn = document.getElementById("forgotBtn");
var loginForgotBtn = document.getElementById("logInForgotBtn");


// Get the <span> elements that close the modals
var closeButtons = document.getElementsByClassName("close");

// When the user clicks a button, open the corresponding modal 
loginBtn.onclick = function () {
    loginModal.style.display = "block";
    signupModal.style.display = "none"; // Close the signup modal
    forgotModal.style.display = "none";
}

loginForgotBtn.onclick = function () {
    loginModal.style.display = "block";
    signupModal.style.display = "none"; // Close the signup modal
    forgotModal.style.display = "none";
}

forgotBtn.onclick = function () {
    forgotModal.style.display = "block";
    loginModal.style.display = "none";
    signupModal.style.display = "none"; // Close the signup modal
}

signupBtn.onclick = function () {
    signupModal.style.display = "block";
    loginModal.style.display = "none"; // Close the login modal
    forgotModal.style.display = "none";
}

// When the user clicks on a <span> (x), close the corresponding modal
for (var i = 0; i < closeButtons.length; i++) {
    closeButtons[i].onclick = function () {
        this.parentElement.parentElement.style.display = "none";
    }
}

// When the user clicks anywhere outside of a modal, close it
window.onclick = function (event) {
    if (event.target == loginModal) {
        loginModal.style.display = "none";
    } else if (event.target == signupModal) {
        signupModal.style.display = "none";
    } else if (event.target == signupModal) {
        forgotModal.style.display = "none";
    }
}

// Get the button that opens the modal
var signupBtn = document.getElementById("signUpBtn");

// When the user clicks the button, open the modal 
signupBtn.onclick = function (event) {
    event.preventDefault(); // Prevent the default action (navigation)
    signupModal.style.display = "block";
    loginModal.style.display = "none"; // Close the login modal
}

// Get the button that opens the modal
var logInBtn = document.getElementById("logInBtn");

// When the user clicks the button, open the modal 
logInBtn.onclick = function (event) {
    event.preventDefault(); // Prevent the default action (navigation)
    loginModal.style.display = "block";
    signupModal.style.display = "none"; // Close the signup modal
}

var slideIndex = 1;
showDivs(slideIndex);

function plusDivs(n) {
    showDivs(slideIndex += n);
}

function currentDiv(n) {
    showDivs(slideIndex = n);
}

function showDivs(n) {
    var i;
    var x = document.getElementsByClassName("mySlides");
    var dots = document.getElementsByClassName("demo");
    if (n > x.length) { slideIndex = 1 }
    if (n < 1) { slideIndex = x.length }
    for (i = 0; i < x.length; i++) {
        x[i].style.display = "none";
    }
    for (i = 0; i < dots.length; i++) {
        dots[i].className = dots[i].className.replace(" w3-white", "");
    }
    x[slideIndex - 1].style.display = "block";
    dots[slideIndex - 1].className += " w3-white";
}

showSlides(slideIndex);

function plusDivs(n) {
    showSlides(slideIndex += n);
}

function currentDiv(n) {
    showSlides(slideIndex = n);
}

function showSlides(n) {
    var i;
    var slides = document.getElementsByClassName("mySlides");
    var dots = document.getElementsByClassName("demo");
    if (n > slides.length) { slideIndex = 1 }
    if (n < 1) { slideIndex = slides.length }
    for (i = 0; i < slides.length; i++) {
        slides[i].style.display = "none";
    }
    for (i = 0; i < dots.length; i++) {
        dots[i].className = dots[i].className.replace(" w3-white", "");
    }
    slides[slideIndex - 1].style.display = "block";
    dots[slideIndex - 1].className += " w3-white";
}

// Auto next slide every 3 seconds
setInterval(function () {
    plusDivs(1);
}, 3000);