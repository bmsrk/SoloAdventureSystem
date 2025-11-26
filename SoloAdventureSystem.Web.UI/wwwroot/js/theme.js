window.theme = (function(){
    function applyTheme(t){
        if(!t) t = 'mud';
        document.body.setAttribute('data-theme', t);
        try { localStorage.setItem('solo_theme', t); } catch(e){}
    }

    function applyFont(f){
        if(!f) f = '';
        // f should be a CSS font-family string
        if(f) document.documentElement.style.setProperty('--font-base', f);
        try { localStorage.setItem('solo_font', f); } catch(e){}
    }

    function init(){
        try{
            var t = localStorage.getItem('solo_theme');
            var f = localStorage.getItem('solo_font');
            if(t) applyTheme(t);
            if(f) applyFont(f);
        }catch(e){ }
    }

    function getSavedTheme(){ try{ return localStorage.getItem('solo_theme') || null; } catch(e){ return null; } }
    function getSavedFont(){ try{ return localStorage.getItem('solo_font') || null; } catch(e){ return null; } }

    return {
        init: init,
        setTheme: applyTheme,
        setFont: applyFont,
        getSavedTheme: getSavedTheme,
        getSavedFont: getSavedFont
    };
})();
