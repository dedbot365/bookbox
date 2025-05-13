// Dashboard Sidebar Functionality
document.addEventListener('DOMContentLoaded', function() {
    const sidebar = document.getElementById('dashboardSidebar');
    const mainContent = document.getElementById('mainContent');
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebarCollapseBtn = document.getElementById('sidebarCollapseBtn');
    const sidebarCollapseInside = document.getElementById('sidebarCollapseInside');
    const sidebarOverlay = document.getElementById('sidebarOverlay');
    
    // Add data-title attributes to all sidebar links for tooltips
    const sidebarLinks = document.querySelectorAll('.sidebar-link');
    sidebarLinks.forEach(link => {
        const text = link.querySelector('span')?.textContent.trim() || '';
        if (text) {
            link.setAttribute('data-title', text);
        }
        
        // Store the original href to use it later
        const originalHref = link.getAttribute('href');
        link.setAttribute('data-href', originalHref);
        
        // Add click handler for handling collapsed sidebar navigation
        link.addEventListener('click', function(e) {
            // Check if sidebar is collapsed (on desktop)
            if (window.innerWidth >= 768 && sidebar.classList.contains('collapsed')) {
                // Prevent the default navigation
                e.preventDefault();
                
                // Expand the sidebar first
                sidebar.classList.remove('collapsed');
                if (mainContent) mainContent.classList.remove('sidebar-collapsed');
                updateCollapseButtonIcon(false);
                localStorage.setItem('sidebarState', 'expanded');
                
                // Get the original href
                const targetHref = this.getAttribute('data-href');
                
                // After a short delay, navigate to the target page
                setTimeout(function() {
                    window.location.href = targetHref;
                }, 300); // 300ms gives enough time for the animation to complete
            } 
            // For mobile view, close sidebar after clicking
            else if (window.innerWidth < 768 && sidebar.classList.contains('show')) {
                // Don't prevent default navigation, just close the sidebar
                setTimeout(function() {
                    sidebar.classList.remove('show');
                    sidebarOverlay.classList.remove('show');
                    sessionStorage.removeItem('sidebarMobileOpen');
                }, 150);
            }
            // Otherwise, just let the default navigation happen (normal expanded state)
        });
    });
    
    // Function to check and set sidebar state based on screen size
    function checkScreenSize() {
        if (window.innerWidth < 768) {
            // Mobile view - check session storage for previous state
            if (sessionStorage.getItem('sidebarMobileOpen') === 'true') {
                sidebar.classList.add('show');
                sidebarOverlay.classList.add('show');
            } else {
                sidebar.classList.remove('show');
                sidebarOverlay.classList.remove('show');
            }
        } else {
            // Desktop view
            const sidebarState = localStorage.getItem('sidebarState');
            if (sidebarState === 'collapsed') {
                sidebar.classList.add('collapsed');
                if (mainContent) mainContent.classList.add('sidebar-collapsed');
                updateCollapseButtonIcon(true);
            } else {
                sidebar.classList.remove('collapsed');
                if (mainContent) mainContent.classList.remove('sidebar-collapsed');
                updateCollapseButtonIcon(false);
            }
        }
    }
    
    // Toggle sidebar on mobile
    function toggleSidebarMobile() {
        const willBeShown = !sidebar.classList.contains('show');
        sidebar.classList.toggle('show');
        sidebarOverlay.classList.toggle('show');
        
        // Store state in session storage
        if (willBeShown) {
            sessionStorage.setItem('sidebarMobileOpen', 'true');
        } else {
            sessionStorage.removeItem('sidebarMobileOpen');
        }
    }
    
    // Toggle sidebar on desktop
    function toggleSidebarDesktop() {
        const willBeCollapsed = !sidebar.classList.contains('collapsed');
        
        sidebar.classList.toggle('collapsed');
        if (mainContent) {
            mainContent.classList.toggle('sidebar-collapsed');
        }
        
        updateCollapseButtonIcon(willBeCollapsed);
        
        // Save preference to localStorage
        localStorage.setItem('sidebarState', willBeCollapsed ? 'collapsed' : 'expanded');
    }
    
    // Update the collapse button icon based on sidebar state
    function updateCollapseButtonIcon(isCollapsed) {
        if (!sidebarCollapseInside) return;
        const collapseIcon = sidebarCollapseInside.querySelector('.collapse-icon');
        if (!collapseIcon) return;
        
        if (isCollapsed) {
            collapseIcon.classList.remove('fa-angle-double-left');
            collapseIcon.classList.add('fa-angle-double-right');
        } else {
            collapseIcon.classList.remove('fa-angle-double-right');
            collapseIcon.classList.add('fa-angle-double-left');
        }
    }
    
    // Event listeners
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function(e) {
            e.preventDefault();
            toggleSidebarMobile();
        });
    }
    
    if (sidebarCollapseBtn) {
        sidebarCollapseBtn.addEventListener('click', function(e) {
            e.preventDefault();
            if (window.innerWidth < 768) {
                toggleSidebarMobile();
            } else {
                toggleSidebarDesktop();
            }
        });
    }
    
    if (sidebarCollapseInside) {
        sidebarCollapseInside.addEventListener('click', function(e) {
            e.preventDefault();
            toggleSidebarDesktop();
        });
    }
    
    if (sidebarOverlay) {
        sidebarOverlay.addEventListener('click', function(e) {
            e.preventDefault();
            toggleSidebarMobile();
        });
    }
    
    // Handle window resize
    window.addEventListener('resize', checkScreenSize);
    
    // Initialize sidebar state
    checkScreenSize();
});