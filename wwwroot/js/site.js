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
    
    // Prepare reveal-on-scroll for pricing/device cards
    document.querySelectorAll('.pricing-card, .device-pricing-card').forEach(el => {
        el.classList.add('reveal-on-scroll', 'tilt-ready', 'glow-hover');
    });
    
    // Initialize counters on BaoGia hero
    initHeroCounters();
    
    // Initialize tilt interactions
    initTiltHover();
    
    // Back-to-top behavior
    initBackToTop();
});

// Re-run animations on page navigation (for SPA-like behavior)
window.addEventListener('pageshow', function() {
    updateActiveNavigation();
    handleScrollAnimations();
});

// ========== BACK TO TOP ==========
function initBackToTop(){
    const btn = document.getElementById('backToTop');
    if(!btn) return;
    const onScroll = () => {
        if(window.scrollY > 300){
            btn.classList.add('show');
        } else {
            btn.classList.remove('show');
        }
    };
    window.addEventListener('scroll', onScroll, { passive: true });
    onScroll();
    btn.addEventListener('click', () => window.scrollTo({ top: 0, behavior: 'smooth' }));
}

// ========== REVEAL ON SCROLL (extend) ==========
// Extend existing observer to support '.reveal-on-scroll'
function handleScrollAnimations() {
    const sections = document.querySelectorAll('.fade-in-section, .reveal-on-scroll');

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

    sections.forEach(section => observer.observe(section));
}

// ========== TILT HOVER ==========
function initTiltHover(){
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;
    const maxTilt = 8; // degrees
    const cards = document.querySelectorAll('.tilt-ready');
    cards.forEach(card => {
        const onMove = (e) => {
            const rect = card.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            const px = (x / rect.width) - 0.5;   // -0.5 -> 0.5
            const py = (y / rect.height) - 0.5;
            const rx = (py * maxTilt * -1).toFixed(2);
            const ry = (px * maxTilt).toFixed(2);
            card.style.transform = `perspective(800px) rotateX(${rx}deg) rotateY(${ry}deg) scale(1.01)`;
            card.classList.add('tilt-hover');
        };
        const reset = () => {
            card.style.transform = '';
            card.classList.remove('tilt-hover');
        };
        card.addEventListener('mousemove', onMove);
        card.addEventListener('mouseleave', reset);
    });
}

// ========== HERO COUNTERS ==========
function initHeroCounters(){
    const counters = document.querySelectorAll(
        '.stat-number[data-target],\
        .contact-hero .stat-number,\
        .pricing-hero .stat-number,\
        .careers-hero .stat-number,\
        .news-hero .stat-number,\
        .devices-hero .stat-number,\
        .about-hero .stat-number'
    );
    if(!counters.length) return;
    const animate = (el) => {
        const raw = (el.textContent || '').trim();
        const hasTarget = el.hasAttribute('data-target');
        const parsed = raw.match(/([0-9]+(?:[\.,][0-9]+)?)/);
        // Prefer data-target when present
        const endValue = hasTarget ? parseFloat(el.getAttribute('data-target')) : (parsed ? parseFloat(parsed[0].replace(',', '.')) : NaN);
        if(Number.isNaN(endValue)) return;
        const numberToken = hasTarget ? (parsed ? parsed[0] : '0') : (parsed ? parsed[0] : '');
        const prefix = raw && numberToken ? raw.slice(0, raw.indexOf(numberToken)) : '';
        const suffix = raw && numberToken ? raw.slice(raw.indexOf(numberToken) + numberToken.length) : '';
        const duration = 2000;
        const start = performance.now();
        const step = (now) => {
            const p = Math.min(1, (now - start) / duration);
            // Easing function: ease-out (starts fast, slows down)
            const easedP = 1 - Math.pow(1 - p, 3);
            const val = Math.floor(endValue * easedP);
            el.textContent = `${prefix}${val.toLocaleString()}${suffix}`;
            if(p < 1) requestAnimationFrame(step);
        };
        requestAnimationFrame(step);
    };
    const io = new IntersectionObserver((entries, obs) => {
        entries.forEach(entry => {
            if(entry.isIntersecting){
                animate(entry.target);
                obs.unobserve(entry.target);
            }
        });
    }, { threshold: 0.4 });
    counters.forEach(c => io.observe(c));
}

// ========== INTERACTIVE SERVICES FUNCTIONALITY ==========
function initInteractiveServices() {
    const serviceItems = document.querySelectorAll('.service-item');
    const serviceImage = document.getElementById('serviceImage');
    const imageLabel = document.getElementById('imageLabel');
    
    if (!serviceItems.length || !serviceImage || !imageLabel) return;
    
    serviceItems.forEach(item => {
        item.addEventListener('click', function() {
            // Remove active class from all items
            serviceItems.forEach(otherItem => {
                otherItem.classList.remove('active');
            });
            
            // Add active class to clicked item
            this.classList.add('active');
            
            // Get new image and label data
            const newImageSrc = this.getAttribute('data-image');
            const newLabel = this.getAttribute('data-label');
            
            // Update image with fade effect
            if (newImageSrc && newImageSrc !== serviceImage.src) {
                serviceImage.style.opacity = '0.7';
                serviceImage.style.transform = 'scale(0.95)';
                
                setTimeout(() => {
                    serviceImage.src = newImageSrc;
                    serviceImage.onload = function() {
                        serviceImage.style.opacity = '1';
                        serviceImage.style.transform = 'scale(1)';
                    };
                }, 200);
            }
            
            // Update label with slide effect
            if (newLabel) {
                const labelText = imageLabel.querySelector('.label-text');
                if (labelText) {
                    labelText.style.opacity = '0';
                    labelText.style.transform = 'translateY(10px)';
                    
                    setTimeout(() => {
                        labelText.textContent = newLabel;
                        labelText.style.opacity = '1';
                        labelText.style.transform = 'translateY(0)';
                    }, 150);
                }
            }
            
            // Add visual feedback
            this.style.transform = 'translateX(8px)';
            setTimeout(() => {
                this.style.transform = '';
            }, 200);
        });
        
        // Add hover effects
        item.addEventListener('mouseenter', function() {
            if (!this.classList.contains('active')) {
                this.style.background = 'rgba(0, 123, 255, 0.03)';
            }
        });
        
        item.addEventListener('mouseleave', function() {
            if (!this.classList.contains('active')) {
                this.style.background = '';
            }
        });
    });
}

// Initialize interactive services when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    // Add delay to ensure all elements are loaded
    setTimeout(() => {
        initInteractiveServices();
    }, 500);
});
