// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('DOMContentLoaded', function () {
    initThemeToggle();
    initAutoDismissAlerts();
    initConfirmModal();
    initPosterPreview();
    initImageMotionEnhancements();
    initLoadingOverlay();
    initBackToTop();
    initTooltips();
    initCopyButtons();
    initPageSizeMemory();
    initListStatePersistence();
    initTableLoadingFeedback();
    applyNavMotionSettings();
    initActiveNav();
    initNavIndicatorSync();
    initSmartNavigation();
});

function applyNavMotionSettings() {
    const nav = document.querySelector('.main-nav');
    if (!nav) return;

    nav.style.setProperty('--nav-indicator-duration', '0.34s');
}

function initThemeToggle() {
    const body = document.body;
    const toggleButton = document.getElementById('themeToggle');
    const savedTheme = localStorage.getItem('vr_theme') || 'light';

    const applyTheme = function (theme) {
        const dark = theme === 'dark';
        body.classList.toggle('dark-theme', dark);

        if (toggleButton) {
            toggleButton.innerText = dark ? '☀️' : '🌙';
            toggleButton.setAttribute('title', dark ? 'Chuyển sang giao diện sáng' : 'Chuyển sang giao diện tối');
            toggleButton.setAttribute('aria-label', dark ? 'Chuyển sang giao diện sáng' : 'Chuyển sang giao diện tối');
        }

        const active = document.querySelector('.main-nav .nav-link.active');
        if (active) {
            requestAnimationFrame(() => moveNavIndicatorToLink(active, true));
            setTimeout(() => moveNavIndicatorToLink(active, true), 80);
        }
    };

    applyTheme(savedTheme === 'dark' ? 'dark' : 'light');

    if (!toggleButton) return;

    toggleButton.addEventListener('click', function () {
        const nextTheme = body.classList.contains('dark-theme') ? 'light' : 'dark';
        applyTheme(nextTheme);
        localStorage.setItem('vr_theme', nextTheme);
    });
}

function initAutoDismissAlerts() {
    const alerts = document.querySelectorAll('.alert.auto-dismiss');
    alerts.forEach(function (alertEl) {
        const progress = alertEl.querySelector('.alert-progress');
        let width = 100;

        const interval = setInterval(function () {
            width -= 2;
            if (progress) progress.style.width = `${Math.max(width, 0)}%`;
            if (width <= 0) {
                clearInterval(interval);
                const bsAlert = bootstrap.Alert.getOrCreateInstance(alertEl);
                bsAlert.close();
            }
        }, 80);
    });
}

function initConfirmModal() {
    const modalElement = document.getElementById('confirmActionModal');
    if (!modalElement) return;

    const messageElement = document.getElementById('confirmActionMessage');
    const confirmButton = document.getElementById('confirmActionButton');
    const modal = new bootstrap.Modal(modalElement);
    let pendingForm = null;

    document.querySelectorAll('form.js-confirm-submit').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            e.preventDefault();
            pendingForm = form;
            const message = form.getAttribute('data-confirm-message') || 'Bạn có chắc muốn thực hiện thao tác này?';
            if (messageElement) messageElement.innerText = message;
            modal.show();
        });
    });

    if (confirmButton) {
        confirmButton.addEventListener('click', function () {
            if (pendingForm) {
                pendingForm.classList.remove('js-confirm-submit');
                pendingForm.submit();
            }
        });
    }
}

function initPosterPreview() {
    const urlInput = document.querySelector('[data-poster-input]');
    const fileInput = document.querySelector('[data-poster-file]');
    const previewImage = document.getElementById('posterPreviewImage');

    if (!urlInput || !previewImage) return;

    const placeholder = previewImage.getAttribute('data-placeholder');

    const setPreview = function (src) {
        previewImage.classList.remove('is-loaded');
        previewImage.src = src || placeholder;
    };

    const updatePreviewFromUrl = function () {
        const value = urlInput.value?.trim();
        if (fileInput && fileInput.files && fileInput.files.length > 0) return;
        setPreview(value || placeholder);
    };

    const updatePreviewFromFile = function (file) {
        if (!file) {
            updatePreviewFromUrl();
            return;
        }

        const reader = new FileReader();
        reader.onload = function (e) {
            setPreview(String(e.target?.result || placeholder));
        };
        reader.readAsDataURL(file);
    };

    urlInput.addEventListener('input', updatePreviewFromUrl);

    if (fileInput) {
        fileInput.addEventListener('change', function () {
            const file = fileInput.files && fileInput.files.length > 0 ? fileInput.files[0] : null;
            updatePreviewFromFile(file);
        });
    }

    previewImage.addEventListener('load', function () {
        previewImage.classList.add('is-loaded');
    });
    previewImage.addEventListener('error', function () {
        previewImage.src = placeholder;
        previewImage.classList.add('is-loaded');
    });

    previewImage.classList.add('media-fade-in');
    updatePreviewFromUrl();
}

function initImageMotionEnhancements() {
    const images = document.querySelectorAll('img.poster-thumb, img.poster-large');

    images.forEach(function (img) {
        img.classList.add('media-fade-in');

        const onLoaded = function () {
            img.classList.add('is-loaded');
        };

        if (!img.hasAttribute('decoding')) {
            img.setAttribute('decoding', 'async');
        }

        if (img.classList.contains('poster-thumb') && !img.hasAttribute('loading')) {
            img.setAttribute('loading', 'lazy');
        }

        if (img.complete && img.naturalWidth > 0) {
            onLoaded();
        } else {
            img.addEventListener('load', onLoaded, { once: true });
            img.addEventListener('error', onLoaded, { once: true });
        }
    });
}

function initLoadingOverlay() {
    const overlay = document.getElementById('loadingOverlay');
    if (!overlay) return;

    const hideOverlay = function () {
        overlay.classList.remove('show');
    };

    let fallbackTimer = null;

    hideOverlay();

    document.querySelectorAll('form').forEach(function (form) {
        form.addEventListener('submit', function () {
            if (form.classList.contains('js-confirm-submit')) return;

            const method = (form.getAttribute('method') || 'get').toLowerCase();
            if (method !== 'post') return;

            overlay.classList.add('show');
            clearTimeout(fallbackTimer);
            fallbackTimer = setTimeout(hideOverlay, 12000);
        });
    });

    window.addEventListener('pageshow', function () {
        clearTimeout(fallbackTimer);
        hideOverlay();
    });

    window.addEventListener('load', function () {
        clearTimeout(fallbackTimer);
        hideOverlay();
    });

    document.addEventListener('visibilitychange', function () {
        if (document.visibilityState === 'visible') {
            clearTimeout(fallbackTimer);
            hideOverlay();
        }
    });
}

function initBackToTop() {
    const btn = document.getElementById('backToTop');
    if (!btn) return;

    const toggle = function () {
        if (window.scrollY > 240) {
            btn.classList.add('show');
        } else {
            btn.classList.remove('show');
        }
    };

    window.addEventListener('scroll', toggle);
    btn.addEventListener('click', function () {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    });

    toggle();
}

function initTooltips() {
    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(function (el) {
        bootstrap.Tooltip.getOrCreateInstance(el, {
            delay: { show: 220, hide: 80 }
        });
    });
}

function initCopyButtons() {
    document.querySelectorAll('.js-copy-btn').forEach(function (button) {
        button.addEventListener('click', async function () {
            const text = button.getAttribute('data-copy-text') || '';
            if (!text) return;

            try {
                await navigator.clipboard.writeText(text);
                showMiniToast('Đã sao chép thông tin phiếu');
            } catch {
                showMiniToast('Không thể sao chép, hãy thử lại', true);
            }
        });
    });
}

function showMiniToast(message, isError = false) {
    const toast = document.createElement('div');
    toast.className = `mini-toast ${isError ? 'error' : ''}`;
    toast.textContent = message;
    document.body.appendChild(toast);

    requestAnimationFrame(() => toast.classList.add('show'));

    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 250);
    }, 1800);
}

function initPageSizeMemory() {
    const select = document.querySelector('.js-page-size-select');
    if (!select) return;

    const saved = localStorage.getItem('vr_rentals_page_size');
    if (saved && select.querySelector(`option[value="${saved}"]`)) {
        select.value = saved;
    }

    select.addEventListener('change', function () {
        localStorage.setItem('vr_rentals_page_size', select.value);
        const form = select.closest('form');
        if (form) {
            form.requestSubmit();
        }
    });
}

function initActiveNav() {
    const currentPath = window.location.pathname.toLowerCase();
    const navLinks = Array.from(document.querySelectorAll('.navbar .nav-link[href]'));

    let bestLink = null;
    let bestLength = -1;

    navLinks.forEach(function (link) {
        link.classList.remove('active');

        const href = link.getAttribute('href');
        if (!href || href === '#') return;

        let normalized = href.toLowerCase();
        if (normalized.length > 1 && normalized.endsWith('/')) {
            normalized = normalized.slice(0, -1);
        }

        const isExact = currentPath === normalized;
        const isPrefix = normalized !== '/' && currentPath.startsWith(`${normalized}/`);

        if ((isExact || isPrefix) && normalized.length > bestLength) {
            bestLink = link;
            bestLength = normalized.length;
        }

        if (normalized === '/' && currentPath === '/' && bestLength < 1) {
            bestLink = link;
            bestLength = 1;
        }
    });

    if (bestLink) {
        bestLink.classList.add('active');
        moveNavIndicatorToLink(bestLink, true);
    }
}

function moveNavIndicatorToLink(link, persistPosition = false) {
    const nav = document.querySelector('.main-nav');
    if (!nav || !link || window.innerWidth < 576) return;

    let indicator = nav.querySelector('.nav-active-indicator');
    if (!indicator) {
        indicator = document.createElement('span');
        indicator.className = 'nav-active-indicator';
        nav.appendChild(indicator);
    }

    const navRect = nav.getBoundingClientRect();
    const linkRect = link.getBoundingClientRect();
    if (!linkRect.width) {
        requestAnimationFrame(() => moveNavIndicatorToLink(link, persistPosition));
        return;
    }

    const x = linkRect.left - navRect.left;
    const width = linkRect.width;

    if (!indicator.classList.contains('ready')) {
        const saved = localStorage.getItem('vr_nav_indicator');
        let previous = null;
        try {
            previous = saved ? JSON.parse(saved) : null;
        } catch {
            previous = null;
        }

        if (previous && Number.isFinite(previous.x) && Number.isFinite(previous.width)) {
            indicator.style.setProperty('--nav-indicator-x', `${previous.x}px`);
            indicator.style.setProperty('--nav-indicator-width', `${previous.width}px`);
            requestAnimationFrame(function () {
                indicator.classList.add('ready');
                indicator.style.setProperty('--nav-indicator-x', `${x}px`);
                indicator.style.setProperty('--nav-indicator-width', `${width}px`);
            });
        } else {
            indicator.style.setProperty('--nav-indicator-x', `${x}px`);
            indicator.style.setProperty('--nav-indicator-width', `${width}px`);
            indicator.classList.add('ready');
        }
    } else {
        indicator.style.setProperty('--nav-indicator-x', `${x}px`);
        indicator.style.setProperty('--nav-indicator-width', `${width}px`);
    }

    if (!indicator.classList.contains('ready')) {
        indicator.classList.add('ready');
    }

    if (persistPosition) {
        localStorage.setItem('vr_nav_indicator', JSON.stringify({ x, width }));
    }
}

function animateNavIndicatorAcrossTabs(startLink, targetLink) {
    const nav = document.querySelector('.main-nav');
    if (!nav || !targetLink || window.innerWidth < 576) {
        moveNavIndicatorToLink(targetLink, false);
        return Promise.resolve();
    }

    const links = Array.from(nav.querySelectorAll('.nav-link[href]'));
    const from = startLink || nav.querySelector('.nav-link.active');

    const startIndex = links.indexOf(from);
    const endIndex = links.indexOf(targetLink);

    if (startIndex < 0 || endIndex < 0 || startIndex === endIndex) {
        moveNavIndicatorToLink(targetLink, false);
        return Promise.resolve();
    }

    const step = endIndex > startIndex ? 1 : -1;
    const distance = Math.abs(endIndex - startIndex);
    const perStep = distance > 2 ? 55 : 75;

    return new Promise((resolve) => {
        let index = startIndex;

        const tick = function () {
            index += step;
            const link = links[index];
            if (link) {
                moveNavIndicatorToLink(link, false);
            }

            if (index === endIndex) {
                setTimeout(resolve, 70);
                return;
            }

            setTimeout(tick, perStep);
        };

        setTimeout(tick, 12);
    });
}

function initNavIndicatorSync() {
    const sync = function () {
        const active = document.querySelector('.main-nav .nav-link.active');
        if (active) {
            moveNavIndicatorToLink(active, true);
        }
    };

    let resizeTimer = null;
    window.addEventListener('resize', function () {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(sync, 90);
    });

    window.addEventListener('orientationchange', function () {
        setTimeout(sync, 120);
    });

    if (document.fonts && document.fonts.ready) {
        document.fonts.ready.then(sync);
    }

    requestAnimationFrame(sync);
    setTimeout(sync, 120);
}

function initSmartNavigation() {
    const prefetched = new Set();

    const shouldHandle = function (link) {
        if (!link) return false;
        const href = link.getAttribute('href');
        if (!href || href.startsWith('#')) return false;
        if (link.target && link.target !== '_self') return false;
        if (link.hasAttribute('download')) return false;

        try {
            const url = new URL(link.href, window.location.origin);
            return url.origin === window.location.origin;
        } catch {
            return false;
        }
    };

    const shouldPrefetch = function (link) {
        return !!link.closest('.main-nav, .pagination, .dashboard-shell, .hero-panel');
    };

    const prefetch = function (link) {
        if (!shouldHandle(link) || !shouldPrefetch(link)) return;

        const href = link.href;
        if (!prefetched.has(href)) {
            const prefetchLink = document.createElement('link');
            prefetchLink.rel = 'prefetch';
            prefetchLink.as = 'document';
            prefetchLink.href = href;
            document.head.appendChild(prefetchLink);
            prefetched.add(href);
        }

        if (href.toLowerCase().includes('/thues/baocao') && !prefetched.has('chartjs-lib')) {
            const libPrefetch = document.createElement('link');
            libPrefetch.rel = 'prefetch';
            libPrefetch.as = 'script';
            libPrefetch.href = 'https://cdn.jsdelivr.net/npm/chart.js';
            document.head.appendChild(libPrefetch);
            prefetched.add('chartjs-lib');
        }
    };

    const getAnchor = function (target) {
        return target?.closest ? target.closest('a[href]') : null;
    };

    document.addEventListener('mouseover', function (e) {
        const link = getAnchor(e.target);
        if (link) prefetch(link);
    }, { passive: true });

    document.addEventListener('focusin', function (e) {
        const link = getAnchor(e.target);
        if (link) prefetch(link);
    }, { passive: true });

    document.addEventListener('touchstart', function (e) {
        const link = getAnchor(e.target);
        if (link) prefetch(link);
    }, { passive: true });

    const startNavigationFeedback = function (link) {
        if (!shouldHandle(link)) return;
        document.body.classList.add('is-navigating');
        document.body.classList.add('nav-indicator-travel');
    };

    document.addEventListener('pointerdown', function (e) {
        const link = getAnchor(e.target);
        if (!link) return;
        startNavigationFeedback(link);
        if (link.closest('.main-nav')) {
            prefetch(link);
        }
    }, { passive: true });

    document.addEventListener('click', function (e) {
        const link = getAnchor(e.target);
        if (!link) return;
        if (!shouldHandle(link)) return;
        if (e.ctrlKey || e.metaKey || e.shiftKey || e.altKey || e.button !== 0) return;

        startNavigationFeedback(link);

        if (link.closest('.main-nav')) {
            const current = window.location.pathname + window.location.search + window.location.hash;
            const target = new URL(link.href, window.location.origin);
            const targetPath = target.pathname + target.search + target.hash;

            if (current !== targetPath) {
                e.preventDefault();
                const nav = document.querySelector('.main-nav');
                const currentActive = nav ? nav.querySelector('.nav-link.active') : null;

                prefetch(link);
                animateNavIndicatorAcrossTabs(currentActive, link).then(function () {
                    window.location.assign(link.href);
                });
            }
            return;
        }
    }, true);

    document.addEventListener('submit', function (e) {
        const form = e.target;
        if (!(form instanceof HTMLFormElement)) return;

        const method = (form.getAttribute('method') || 'get').toLowerCase();
        if (method === 'get') {
            document.body.classList.add('is-navigating');
        }
    }, true);

    const finishNavigationFeedback = function () {
        document.body.classList.remove('is-navigating');
        document.body.classList.remove('nav-indicator-travel');
        const progress = document.getElementById('navProgress');
        if (!progress) return;

        progress.style.transform = 'scaleX(1)';
        requestAnimationFrame(function () {
            setTimeout(function () {
                progress.style.transform = 'scaleX(0)';
            }, 90);
        });
    };

    window.addEventListener('pageshow', finishNavigationFeedback);
    window.addEventListener('load', finishNavigationFeedback);
    document.addEventListener('visibilitychange', function () {
        if (document.visibilityState === 'visible') {
            finishNavigationFeedback();
        }
    });
}

function initListStatePersistence() {
    const forms = document.querySelectorAll('form.js-stateful-filter');

    forms.forEach(function (form) {
        const key = `vr_list_state_${form.getAttribute('data-list-key') || window.location.pathname}`;

        try {
            const raw = sessionStorage.getItem(key);
            if (raw) {
                const state = JSON.parse(raw);
                Object.entries(state.values || {}).forEach(([name, value]) => {
                    const field = form.elements.namedItem(name);
                    if (!field) return;
                    if (field instanceof RadioNodeList) {
                        Array.from(field).forEach(el => {
                            if (el.value === value) el.checked = true;
                        });
                    } else if (field instanceof HTMLInputElement || field instanceof HTMLSelectElement) {
                        field.value = value;
                    }
                });

                if (Number.isFinite(state.scrollY)) {
                    requestAnimationFrame(() => window.scrollTo({ top: state.scrollY, behavior: 'auto' }));
                }
            }
        } catch {
            // no-op
        }

        form.addEventListener('submit', function () {
            const values = {};
            new FormData(form).forEach((v, k) => {
                values[k] = String(v);
            });

            const payload = {
                values,
                scrollY: window.scrollY
            };

            sessionStorage.setItem(key, JSON.stringify(payload));
        });
    });
}

function initTableLoadingFeedback() {
    const activateByElement = function (element) {
        if (!element) return;

        const localShell = element.closest('.table-shell');
        if (localShell) {
            localShell.classList.add('is-loading');
            return;
        }

        const localContainer = element.closest('.card, .glass-panel, .page-surface, .container');
        const shell = localContainer ? localContainer.querySelector('.table-shell') : document.querySelector('.table-shell');
        if (shell) {
            shell.classList.add('is-loading');
        }
    };

    document.querySelectorAll('.pagination .page-link').forEach(function (link) {
        link.addEventListener('click', function () {
            activateByElement(link);
        });
    });

    document.querySelectorAll('form.js-stateful-filter').forEach(function (form) {
        form.addEventListener('submit', function () {
            activateByElement(form);
        });
    });

    window.addEventListener('pageshow', function () {
        document.querySelectorAll('.table-shell.is-loading').forEach(function (shell) {
            shell.classList.remove('is-loading');
        });
    });
}
