"use client";

import { useState, useRef, useCallback, useEffect } from "react";
import Image from "next/image";
import { ChevronLeft, ChevronRight, ZoomIn, ZoomOut, Maximize2, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent } from "@/components/ui/dialog";
import { getSafeImageUrl } from "@/lib/utils";

interface ProductImage {
  id: string;
  url: string;
  altText?: string;
  sortOrder: number;
  isFeatured: boolean;
}

interface ProductImageGalleryProps {
  images: ProductImage[];
  productName: string;
}

export function ProductImageGallery({ images, productName }: ProductImageGalleryProps) {
  const [selectedIndex, setSelectedIndex] = useState(0);
  const [isZoomed, setIsZoomed] = useState(false);
  const [zoomPosition, setZoomPosition] = useState({ x: 50, y: 50 });
  const [isFullscreen, setIsFullscreen] = useState(false);
  const [fullscreenIndex, setFullscreenIndex] = useState(0);
  const [touchStart, setTouchStart] = useState<number | null>(null);
  const imageContainerRef = useRef<HTMLDivElement>(null);

  const currentImage = images[selectedIndex];

  const goToPrevious = useCallback(() => {
    setSelectedIndex(prev => (prev === 0 ? images.length - 1 : prev - 1));
  }, [images.length]);

  const goToNext = useCallback(() => {
    setSelectedIndex(prev => (prev === images.length - 1 ? 0 : prev + 1));
  }, [images.length]);

  const goToFullscreenPrevious = useCallback(() => {
    setFullscreenIndex(prev => (prev === 0 ? images.length - 1 : prev - 1));
  }, [images.length]);

  const goToFullscreenNext = useCallback(() => {
    setFullscreenIndex(prev => (prev === images.length - 1 ? 0 : prev + 1));
  }, [images.length]);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (isFullscreen) {
        if (e.key === "ArrowLeft") goToFullscreenPrevious();
        else if (e.key === "ArrowRight") goToFullscreenNext();
        else if (e.key === "Escape") setIsFullscreen(false);
      } else {
        if (e.key === "ArrowLeft") goToPrevious();
        else if (e.key === "ArrowRight") goToNext();
      }
    };
    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [isFullscreen, goToPrevious, goToNext, goToFullscreenPrevious, goToFullscreenNext]);

  const handleMouseMove = useCallback((e: React.MouseEvent<HTMLDivElement>) => {
    if (!isZoomed) return;
    const rect = e.currentTarget.getBoundingClientRect();
    const x = ((e.clientX - rect.left) / rect.width) * 100;
    const y = ((e.clientY - rect.top) / rect.height) * 100;
    setZoomPosition({ x, y });
  }, [isZoomed]);

  const handleTouchStart = useCallback((e: React.TouchEvent) => {
    setTouchStart(e.touches[0].clientX);
  }, []);

  const handleTouchEnd = useCallback((e: React.TouchEvent) => {
    if (touchStart === null) return;
    const diff = touchStart - e.changedTouches[0].clientX;
    if (Math.abs(diff) > 50) {
      if (diff > 0) goToNext();
      else goToPrevious();
    }
    setTouchStart(null);
  }, [touchStart, goToNext, goToPrevious]);

  if (images.length === 0) return null;

  return (
    <>
      <div className="space-y-4">
        {/* Main Image */}
        <div
          ref={imageContainerRef}
          className="relative aspect-[3/4] overflow-hidden rounded-lg bg-muted cursor-zoom-in"
          onClick={() => {
            if (isZoomed) {
              setIsZoomed(false);
            } else {
              setIsZoomed(true);
            }
          }}
          onMouseMove={handleMouseMove}
          onMouseLeave={() => setIsZoomed(false)}
          onTouchStart={handleTouchStart}
          onTouchEnd={handleTouchEnd}
          role="img"
          aria-label={`${productName} - image ${selectedIndex + 1} of ${images.length}`}
          tabIndex={0}
        >
          <Image
            src={getSafeImageUrl(currentImage?.url)}
            alt={currentImage?.altText || productName}
            fill
            className={`object-cover transition-transform duration-200 ${isZoomed ? "scale-[2.5]" : "scale-100"}`}
            style={isZoomed ? { transformOrigin: `${zoomPosition.x}% ${zoomPosition.y}%` } : undefined}
            sizes="(max-width: 1024px) 100vw, 50vw"
            priority
          />

          {/* Zoom indicator */}
          <div className="absolute right-3 top-3 z-10">
            <Button
              variant="secondary"
              size="icon"
              className="h-8 w-8 bg-background/80 backdrop-blur-sm"
              onClick={(e) => {
                e.stopPropagation();
                setIsZoomed(!isZoomed);
              }}
              aria-label={isZoomed ? "Zoom out" : "Zoom in"}
            >
              {isZoomed ? <ZoomOut className="h-4 w-4" /> : <ZoomIn className="h-4 w-4" />}
            </Button>
          </div>

          {/* Fullscreen button */}
          <div className="absolute right-3 top-14 z-10">
            <Button
              variant="secondary"
              size="icon"
              className="h-8 w-8 bg-background/80 backdrop-blur-sm"
              onClick={(e) => {
                e.stopPropagation();
                setFullscreenIndex(selectedIndex);
                setIsFullscreen(true);
              }}
              aria-label="View fullscreen"
            >
              <Maximize2 className="h-4 w-4" />
            </Button>
          </div>

          {/* Navigation arrows */}
          {images.length > 1 && (
            <>
              <Button
                variant="secondary"
                size="icon"
                className="absolute left-3 top-1/2 h-8 w-8 -translate-y-1/2 bg-background/80 backdrop-blur-sm"
                onClick={(e) => {
                  e.stopPropagation();
                  goToPrevious();
                }}
                aria-label="Previous image"
              >
                <ChevronLeft className="h-4 w-4" />
              </Button>
              <Button
                variant="secondary"
                size="icon"
                className="absolute right-3 top-1/2 h-8 w-8 -translate-y-1/2 bg-background/80 backdrop-blur-sm"
                onClick={(e) => {
                  e.stopPropagation();
                  goToNext();
                }}
                aria-label="Next image"
              >
                <ChevronRight className="h-4 w-4" />
              </Button>
            </>
          )}

          {/* Image counter */}
          <div className="absolute bottom-3 left-1/2 -translate-x-1/2 rounded-full bg-background/80 px-3 py-1 text-xs backdrop-blur-sm">
            {selectedIndex + 1} / {images.length}
          </div>
        </div>

        {/* Thumbnails */}
        {images.length > 1 && (
          <div className="flex gap-2 overflow-x-auto pb-2" role="tablist" aria-label="Product images">
            {images.map((image, index) => (
              <button
                key={image.id}
                role="tab"
                aria-selected={selectedIndex === index}
                aria-label={`View image ${index + 1}`}
                onClick={() => setSelectedIndex(index)}
                className={`relative h-20 w-20 shrink-0 overflow-hidden rounded-md border-2 transition-all ${
                  selectedIndex === index
                    ? "border-primary"
                    : "border-transparent hover:border-muted-foreground/50"
                }`}
              >
                <Image
                  src={getSafeImageUrl(image.url)}
                  alt={image.altText || `${productName} ${index + 1}`}
                  fill
                  className="object-cover"
                  sizes="80px"
                />
              </button>
            ))}
          </div>
        )}
      </div>

      {/* Fullscreen Dialog */}
      <Dialog open={isFullscreen} onOpenChange={setIsFullscreen}>
        <DialogContent className="max-w-[95vw] max-h-[95vh] p-0 bg-black border-0" aria-label="Fullscreen image viewer">
          <div className="relative aspect-[3/4] max-h-[90vh] w-full">
            <Image
              src={getSafeImageUrl(images[fullscreenIndex]?.url)}
              alt={images[fullscreenIndex]?.altText || productName}
              fill
              className="object-contain"
              sizes="95vw"
            />

            <Button
              variant="ghost"
              size="icon"
              className="absolute right-2 top-2 h-10 w-10 text-white hover:bg-white/20"
              onClick={() => setIsFullscreen(false)}
              aria-label="Close fullscreen"
            >
              <X className="h-6 w-6" />
            </Button>

            {images.length > 1 && (
              <>
                <Button
                  variant="ghost"
                  size="icon"
                  className="absolute left-2 top-1/2 h-10 w-10 -translate-y-1/2 text-white hover:bg-white/20"
                  onClick={goToFullscreenPrevious}
                  aria-label="Previous image"
                >
                  <ChevronLeft className="h-6 w-6" />
                </Button>
                <Button
                  variant="ghost"
                  size="icon"
                  className="absolute right-2 top-1/2 h-1/2 -translate-y-1/2 text-white hover:bg-white/20"
                  onClick={goToFullscreenNext}
                  aria-label="Next image"
                >
                  <ChevronRight className="h-6 w-6" />
                </Button>
              </>
            )}

            <div className="absolute bottom-4 left-1/2 -translate-x-1/2 rounded-full bg-black/60 px-4 py-2 text-sm text-white backdrop-blur-sm">
              {fullscreenIndex + 1} / {images.length}
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </>
  );
}
