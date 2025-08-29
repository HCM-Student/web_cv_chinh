// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// ========== PAGE TRANSITION EFFECTS ==========

// Simple page loading
document.addEventListener('DOMContentLoaded', function() {
    // Simple fade in when page loads
    document.body.style.opacity = '0';
    document.body.style.transition = 'opacity 0.4s ease-in-out';
    
    window.addEventListener('load', function() {
        setTimeout(() => {
            document.body.style.opacity = '1';
            document.body.classList.add('loaded');
        }, 100);
    });
});

// Simple page transitions for navigation
document.addEventListener('DOMContentLoaded', function() {
    const navLinks = document.querySelectorAll('.navbar-nav .nav-link');
    
    navLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            const href = this.getAttribute('href');
            
            // Skip if it's just an anchor link
            if (!href || href.startsWith('#') || href.startsWith('http')) return;
            
            e.preventDefault();
            
            // Simple fade effect
            document.body.style.opacity = '0.7';
            
            setTimeout(() => {
                window.location.href = href;
            }, 150);
        });
    });
});

// Scroll animations
function handleScrollAnimations() {
    const sections = document.querySelectorAll('.fade-in-section');
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('is-visible');
            }
        });
    }, {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    });
    
    sections.forEach(section => {
        observer.observe(section);
    });
}

// Enhanced navigation active state
function updateActiveNavigation() {
    const currentPath = window.location.pathname.toLowerCase();
    const navLinks = document.querySelectorAll('.navbar-nav .nav-link');
    
    navLinks.forEach(link => {
        link.classList.remove('active');
        const href = link.getAttribute('href');
        
        if (href && currentPath.includes(href.toLowerCase().replace('/', ''))) {
            link.classList.add('active');
        }
    });
}

// Smooth scrolling for anchor links
function initSmoothScrolling() {
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });
}

// Enhanced button ripple effect
function initButtonRipples() {
    document.querySelectorAll('.btn').forEach(button => {
        button.addEventListener('click', function(e) {
            const ripple = document.createElement('span');
            const rect = this.getBoundingClientRect();
            const size = Math.max(rect.width, rect.height);
            const x = e.clientX - rect.left - size / 2;
            const y = e.clientY - rect.top - size / 2;
            
            ripple.style.width = ripple.style.height = size + 'px';
            ripple.style.left = x + 'px';
            ripple.style.top = y + 'px';
            ripple.classList.add('ripple');
            
            this.appendChild(ripple);
            
            setTimeout(() => {
                ripple.remove();
            }, 600);
        });
    });
}

// Initialize all effects
document.addEventListener('DOMContentLoaded', function() {
    handleScrollAnimations();
    updateActiveNavigation();
    initSmoothScrolling();
    initButtonRipples();
    
    // Add page-container class to main content
    const mainContent = document.querySelector('main, .container, .content');
    if (mainContent) {
        mainContent.classList.add('page-container');
    }
});

// Re-run animations on page navigation (for SPA-like behavior)
window.addEventListener('pageshow', function() {
    updateActiveNavigation();
    handleScrollAnimations();
});
