(function(){
  var cfg = window.layoutConfig || {};
  var variant   = cfg.variant    || 'app';
  var username  = cfg.username   || 'User';
  var navLinks  = cfg.navLinks   || [];
  var activePage= cfg.activePage || '';
  var footerText= cfg.footerText || 'Project Builder v3.0.2';
  var logoSrc   = cfg.logoSrc    || 'shared/images/logo.png';
  var logo2Src  = cfg.logo2Src   || 'shared/images/SecondaryLogo.svg';
  var loginHref = cfg.loginHref  || 'signin.html';
  var logoutHref= cfg.logoutHref || 'index.html';

  function buildLogos(){
    return '<div class="logos-container">' +
      '<img src="' + logoSrc + '" alt="PennDOT" style="height:70px;object-fit:contain"/>' +
      '<div class="verticalline"></div>' +
      '<img src="' + logo2Src + '" alt="Project Builder" style="height:70px;object-fit:contain"/>' +
    '</div>';
  }

  function buildNavLinks(){
    if(!navLinks.length) return '';
    var html = '<div class="navbar_navigation-container"><ul class="navbar_navigation-links">';
    navLinks.forEach(function(link){
      var cls = link.key === activePage ? 'active' : 'notactive';
      html += '<li><a href="' + link.href + '" class="navbar_navigation-link ' + cls + '">' +
        '<div class="navbar_navigation-link_item"><span>' + link.label + '</span></div>' +
      '</a></li>';
    });
    html += '</ul></div>';
    return html;
  }

  function buildUserInfo(){
    return '<div class="navbar_userinfo-container">' +
      '<i class="fa-solid fa-user"></i>' +
      '<p>' + username + '</p>' +
      '<div class="verticalline"></div>' +
      '<a href="' + logoutHref + '" class="circle" title="Logout"><i class="fa-solid fa-right-from-bracket"></i></a>' +
    '</div>';
  }

  function buildSignIn(){
    return '<div class="navbar_userinfo-container">' +
      '<button class="btn-signin-nav" onclick="location.href=\'' + loginHref + '\'">Sign in</button>' +
    '</div>';
  }

  function injectNavbar(){
    var nav = document.createElement('nav');
    nav.id = 'main-navbar';
    var inner = '<div class="navbar_maincontainer">' + buildLogos();
    if(variant === 'app'){
      inner += buildNavLinks() + buildUserInfo();
    } else {
      inner += buildSignIn();
    }
    inner += '</div>';
    nav.innerHTML = inner;
    document.body.insertBefore(nav, document.body.firstChild);
  }

  function injectFooter(){
    var footer = document.createElement('footer');
    footer.innerHTML = '<div class="app-footer"><div class="app-footer__content">' + footerText + '</div></div>';
    document.body.appendChild(footer);
  }

  function init(){
    if(!document.getElementById('main-navbar')) injectNavbar();
    if(!document.querySelector('.app-footer'))  injectFooter();
  }

  if(document.readyState === 'loading'){
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();
