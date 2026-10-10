#!/usr/bin/env python3
"""Render an original Expanded Summoning creature icon source with Blender.

Run headless:

    blender --background --factory-startup --python render_creature_icon.py \
        -- --creature pony --out sources/pony.png [--samples 128]

Everything here is procedural and project-owned: the creature is built from
metaballs and mesh primitives with procedural materials, lit with a warm key,
a cool rim and a low fill, in front of a radial atmospheric backdrop inside a
dark bronze ring, in the framing the roster icons established. No game pixels,
downloaded model, texture or generative-model output is an input. The output is
the 1254 by 1254 RGBA source that tools/New-ExpandedSummoningIcons.ps1 turns
into the 128 by 128 production icon.

Metaball note: a metaball element renders smaller than its nominal radius
(about three quarters of it at the default threshold), so the radii below are
authored for the rendered size, not the nominal one.
"""
import argparse
import math
import sys

import bpy  # noqa: E402
import mathutils  # noqa: E402
from mathutils import Vector, Euler  # noqa: E402


SIZE = 1254
UP = Vector((0.0, 0.0, 1.0))


# --- scene helpers ---------------------------------------------------------------
def clear_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    scene.render.engine = "CYCLES"
    scene.cycles.device = "CPU"
    scene.render.resolution_x = SIZE
    scene.render.resolution_y = SIZE
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.image_settings.color_mode = "RGBA"
    scene.render.image_settings.color_depth = "8"
    scene.render.film_transparent = False
    scene.view_settings.view_transform = "AgX"
    scene.view_settings.look = "AgX - Medium High Contrast"
    scene.cycles.use_denoising = True
    scene.cycles.denoiser = "OPENIMAGEDENOISE"
    scene.cycles.max_bounces = 6
    world = bpy.data.worlds.new("IconWorld")
    scene.world = world
    world.use_nodes = True
    background = world.node_tree.nodes["Background"]
    background.inputs["Color"].default_value = (0.02, 0.02, 0.025, 1.0)
    background.inputs["Strength"].default_value = 0.5
    return scene


def material(name, color, roughness=0.55, metallic=0.0, emission=None,
             emission_strength=0.0, subsurface=0.0, noise=None):
    """A principled material. `noise` = (scale, strength, darker colour)
    adds a procedural mottle so flat primitives read as hide, fur or skin."""
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    tree = mat.node_tree
    principled = tree.nodes["Principled BSDF"]
    principled.inputs["Base Color"].default_value = (*color, 1.0)
    principled.inputs["Roughness"].default_value = roughness
    principled.inputs["Metallic"].default_value = metallic
    if subsurface:
        principled.inputs["Subsurface Weight"].default_value = subsurface
        principled.inputs["Subsurface Radius"].default_value = (0.4, 0.2, 0.1)
    if emission is not None:
        principled.inputs["Emission Color"].default_value = (*emission, 1.0)
        principled.inputs["Emission Strength"].default_value = emission_strength
    if noise is not None:
        scale, strength, darker = noise
        tex = tree.nodes.new("ShaderNodeTexNoise")
        tex.inputs["Scale"].default_value = scale
        tex.inputs["Detail"].default_value = 7.0
        tex.inputs["Roughness"].default_value = 0.72
        ramp = tree.nodes.new("ShaderNodeValToRGB")
        ramp.color_ramp.elements[0].position = 0.38
        ramp.color_ramp.elements[0].color = (*darker, 1.0)
        ramp.color_ramp.elements[1].position = 0.7
        ramp.color_ramp.elements[1].color = (*color, 1.0)
        mix = tree.nodes.new("ShaderNodeMix")
        mix.data_type = "RGBA"
        mix.inputs["Factor"].default_value = strength
        mix.inputs[6].default_value = (*color, 1.0)
        tree.links.new(tex.outputs["Fac"], ramp.inputs["Fac"])
        tree.links.new(ramp.outputs["Color"], mix.inputs[7])
        tree.links.new(mix.outputs[2], principled.inputs["Base Color"])
    return mat


def assign(obj, mat):
    obj.data.materials.clear()
    obj.data.materials.append(mat)


def smooth(obj, levels=2):
    for polygon in obj.data.polygons:
        polygon.use_smooth = True
    if levels:
        modifier = obj.modifiers.new("Subdivision", "SUBSURF")
        modifier.levels = levels
        modifier.render_levels = levels


def sphere(name, location, scale, mat, rotation=(0, 0, 0), segments=48):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=segments // 2,
                                         location=location)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = scale
    obj.rotation_euler = Euler([math.radians(v) for v in rotation], "XYZ")
    smooth(obj, 1)
    assign(obj, mat)
    return obj


def cylinder(name, location, radius, depth, mat, rotation=(0, 0, 0), vertices=48):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth,
                                        location=location)
    obj = bpy.context.active_object
    obj.name = name
    obj.rotation_euler = Euler([math.radians(v) for v in rotation], "XYZ")
    smooth(obj, 1)
    assign(obj, mat)
    return obj


def cone(name, location, radius, depth, mat, rotation=(0, 0, 0), radius2=0.0):
    bpy.ops.mesh.primitive_cone_add(vertices=48, radius1=radius, radius2=radius2,
                                    depth=depth, location=location)
    obj = bpy.context.active_object
    obj.name = name
    obj.rotation_euler = Euler([math.radians(v) for v in rotation], "XYZ")
    smooth(obj, 0)
    assign(obj, mat)
    return obj


def cone_along(name, start, direction, length, radius, mat, radius2=0.0):
    """A cone whose base sits at `start` and whose tip points along `direction`."""
    direction = Vector(direction).normalized()
    centre = Vector(start) + direction * (length / 2.0)
    bpy.ops.mesh.primitive_cone_add(vertices=48, radius1=radius, radius2=radius2,
                                    depth=length, location=centre)
    obj = bpy.context.active_object
    obj.name = name
    obj.rotation_euler = direction.to_track_quat("Z", "Y").to_euler()
    smooth(obj, 0)
    assign(obj, mat)
    return obj


def box(name, location, scale, mat, rotation=(0, 0, 0), bevel=0.0):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=location)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = scale
    obj.rotation_euler = Euler([math.radians(v) for v in rotation], "XYZ")
    if bevel > 0:
        modifier = obj.modifiers.new("Bevel", "BEVEL")
        modifier.width = bevel
        modifier.segments = 4
    for polygon in obj.data.polygons:
        polygon.use_smooth = bevel > 0
    assign(obj, mat)
    return obj


class Blob:
    """One metaball object: organic masses that merge into each other."""

    def __init__(self, name, mat, resolution=0.05):
        data = bpy.data.metaballs.new(name)
        data.resolution = resolution
        data.render_resolution = resolution
        data.threshold = 0.6
        self.obj = bpy.data.objects.new(name, data)
        bpy.context.collection.objects.link(self.obj)
        data.materials.append(mat)
        self.data = data

    def ball(self, location, radius, size=None, axis=None, stiffness=2.0):
        """A ball, or an ellipsoid whose local X follows `axis` and whose
        `size` scales (along axis, across, up). Radii are rendered sizes."""
        element = self.data.elements.new()
        element.type = "ELLIPSOID" if size else "BALL"
        element.co = Vector(location)
        element.radius = radius / 0.75
        element.stiffness = stiffness
        if size:
            element.size_x, element.size_y, element.size_z = size
        if axis is not None:
            element.rotation = Vector(axis).normalized().to_track_quat("X", "Z")
        return element

    def chain(self, start, end, radius_start, radius_end, count=8, stiffness=2.2):
        start, end = Vector(start), Vector(end)
        for index in range(count):
            t = index / (count - 1)
            self.ball(start.lerp(end, t), radius_start + (radius_end - radius_start) * t,
                      stiffness=stiffness)


def backdrop(inner, outer, cam_location, cam_target, lens,
             ring_color=(0.30, 0.19, 0.08)):
    """Radial atmospheric background plane and the dark bronze ring, both
    centred on the camera's view line and facing the camera, the ring sized
    to sit just inside the frame edge as the roster icons' ring does."""
    origin = Vector(cam_location)
    view = (Vector(cam_target) - origin).normalized()
    facing = (-view).to_track_quat("Z", "Y").to_euler()
    plane_centre = origin + view * ((6.0 - origin.y) / view.y)
    bpy.ops.mesh.primitive_plane_add(size=40.0, location=plane_centre,
                                     rotation=facing)
    plane = bpy.context.active_object
    plane.name = "Backdrop"
    mat = bpy.data.materials.new("BackdropMaterial")
    mat.use_nodes = True
    tree = mat.node_tree
    tree.nodes.clear()
    output = tree.nodes.new("ShaderNodeOutputMaterial")
    emission = tree.nodes.new("ShaderNodeEmission")
    emission.inputs["Strength"].default_value = 1.0
    coords = tree.nodes.new("ShaderNodeTexCoord")
    mapping = tree.nodes.new("ShaderNodeMapping")
    mapping.inputs["Scale"].default_value = (0.22, 0.22, 0.22)
    gradient = tree.nodes.new("ShaderNodeTexGradient")
    gradient.gradient_type = "SPHERICAL"
    ramp = tree.nodes.new("ShaderNodeValToRGB")
    ramp.color_ramp.elements[0].position = 0.0
    ramp.color_ramp.elements[0].color = (*outer, 1.0)
    ramp.color_ramp.elements[1].position = 0.9
    ramp.color_ramp.elements[1].color = (*inner, 1.0)
    cloud = tree.nodes.new("ShaderNodeTexNoise")
    cloud.inputs["Scale"].default_value = 1.6
    cloud.inputs["Detail"].default_value = 8.0
    mix = tree.nodes.new("ShaderNodeMix")
    mix.data_type = "RGBA"
    mix.blend_type = "MULTIPLY"
    mix.inputs["Factor"].default_value = 0.35
    tree.links.new(coords.outputs["Object"], mapping.inputs["Vector"])
    tree.links.new(mapping.outputs["Vector"], gradient.inputs["Vector"])
    tree.links.new(gradient.outputs["Fac"], ramp.inputs["Fac"])
    tree.links.new(coords.outputs["Object"], cloud.inputs["Vector"])
    tree.links.new(ramp.outputs["Color"], mix.inputs[6])
    tree.links.new(cloud.outputs["Color"], mix.inputs[7])
    tree.links.new(mix.outputs[2], emission.inputs["Color"])
    tree.links.new(emission.outputs["Emission"], output.inputs["Surface"])
    assign(plane, mat)
    bronze = material("Bronze", ring_color, roughness=0.42, metallic=1.0)
    ring_distance = (3.2 - origin.y) / view.y
    ring_centre = origin + view * ring_distance
    ring_radius = 0.90 * ring_distance * 18.0 / lens
    bpy.ops.mesh.primitive_torus_add(major_radius=ring_radius,
                                     minor_radius=0.026 * ring_radius,
                                     major_segments=160, minor_segments=24,
                                     location=ring_centre, rotation=facing)
    ring = bpy.context.active_object
    ring.name = "Ring"
    smooth(ring, 0)
    assign(ring, bronze)


def lights(key=(1.0, 0.85, 0.6), rim=(0.55, 0.75, 1.0), key_energy=420.0,
           rim_energy=380.0, fill_energy=70.0):
    def lamp(name, kind, location, color, energy, size=2.0):
        data = bpy.data.lights.new(name, kind)
        data.energy = energy
        data.color = color
        if kind == "AREA":
            data.size = size
        obj = bpy.data.objects.new(name, data)
        bpy.context.collection.objects.link(obj)
        obj.location = location
        direction = Vector((0, 0, 1.0)) - obj.location
        obj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()
        return obj
    lamp("Key", "AREA", (-3.2, -3.5, 4.2), key, key_energy, 2.5)
    lamp("Rim", "AREA", (3.0, 2.6, 3.4), rim, rim_energy, 1.5)
    lamp("Fill", "AREA", (2.8, -4.0, 0.4), (0.8, 0.85, 1.0), fill_energy, 4.0)


def camera(location=(0.0, -7.2, 1.15), target=(0.0, 0.0, 1.0), lens=60.0):
    data = bpy.data.cameras.new("IconCamera")
    data.lens = lens
    obj = bpy.data.objects.new("IconCamera", data)
    bpy.context.collection.objects.link(obj)
    obj.location = location
    direction = Vector(target) - Vector(location)
    obj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()
    bpy.context.scene.camera = obj
    return obj


# --- creatures ---------------------------------------------------------------------
def equine(pony):
    """Head, neck and chest of a pony (stocky, shaggy) or a horse (long,
    refined), three-quarter from the front, the face turned to the camera's
    left."""
    if pony:
        coat = material("Coat", (0.42, 0.27, 0.13), 0.65,
                        noise=(9.0, 0.5, (0.28, 0.17, 0.08)))
        mane = material("Mane", (0.14, 0.095, 0.06), 0.85,
                        noise=(16.0, 0.55, (0.05, 0.035, 0.022)))
        muzzle_color = (0.24, 0.16, 0.10)
        neck_len, neck_r0, neck_r1, head_len = 1.35, 0.72, 0.5, 1.3
        poll = Vector((-0.45, -0.1, 1.35))
    else:
        coat = material("Coat", (0.33, 0.13, 0.05), 0.55,
                        noise=(9.0, 0.45, (0.20, 0.08, 0.035)))
        mane = material("Mane", (0.07, 0.05, 0.035), 0.8,
                        noise=(16.0, 0.5, (0.025, 0.02, 0.015)))
        muzzle_color = (0.14, 0.09, 0.06)
        neck_len, neck_r0, neck_r1, head_len = 1.8, 0.58, 0.4, 1.55
        poll = Vector((-0.5, -0.15, 1.75))
    muzzle_mat = material("Muzzle", muzzle_color, 0.6)
    axis = Vector((-0.62, -0.5, -0.62)).normalized()      # poll to muzzle
    side = axis.cross(UP).normalized()                     # to the creature's right
    muzzle = poll + axis * head_len
    chest = Vector((0.4, 0.4, -0.05))
    body = Blob("Body", coat, 0.045)
    body.ball((0.45, 0.6, -0.95), 1.3, (1.35, 1.0, 0.85), axis=(1, 0, 0))
    body.ball((-0.35, 0.35, -0.8), 0.9)
    body.chain(chest, poll, neck_r0, neck_r1, count=9)
    head = Blob("Head", coat, 0.04)
    head.ball(poll + axis * (0.3 * head_len), 0.62, (1.25, 0.82, 0.95), axis=axis)
    head.ball(poll + axis * (0.52 * head_len) - UP * 0.12, 0.52, (1.25, 0.8, 0.85),
              axis=axis)
    head.ball(muzzle - axis * 0.28, 0.42, (1.0, 0.85, 0.85), axis=axis)
    head.ball(poll + axis * 0.1, 0.5, (0.9, 0.9, 0.9))
    sphere("Muzzle", muzzle - axis * 0.08, (0.3, 0.3, 0.26), muzzle_mat)
    nostril = material("Nostril", (0.03, 0.02, 0.02), 0.5)
    for s in (-1, 1):
        sphere("Nostril%d" % s, muzzle - axis * 0.02 + side * (s * 0.16) - UP * 0.02,
               (0.06, 0.05, 0.07), nostril)
    eye = material("Eye", (0.02, 0.015, 0.01), 0.12)
    eye_center = poll + axis * (0.34 * head_len) + UP * 0.1
    sphere("EyeL", eye_center - side * 0.46, (0.11, 0.1, 0.1), eye)
    sphere("EyeR", eye_center + side * 0.46, (0.11, 0.1, 0.1), eye)
    ear_len = 0.5 if pony else 0.58
    for s in (-1, 1):
        base = poll + side * (s * 0.26) + UP * 0.05
        cone_along("Ear%d" % s, base, Vector((0.0, -0.15, 1.0)) + side * (s * 0.35),
                   ear_len, 0.13, coat)
    # mane along the crest of the neck, forelock between the ears
    neck_dir = (poll - chest).normalized()
    crest = (UP - neck_dir * UP.dot(neck_dir)).normalized()
    hair = Blob("Mane", mane, 0.045)
    count = 11
    for index in range(count):
        t = index / (count - 1)
        wave = 0.12 * math.sin(index * 1.9)
        centre = chest.lerp(poll, t) + crest * ((0.46 if pony else 0.36) + wave)
        hair.ball(centre, (0.40 if pony else 0.27) * (1.0 - 0.15 * t), stiffness=2.4)
        # a second, lower row hanging down the near side of the neck
        hair.ball(centre - crest * 0.25 - side * (0.28 if pony else 0.2),
                  (0.26 if pony else 0.18) * (1.0 - 0.15 * t), stiffness=2.6)
    hair.ball(poll + crest * 0.25 + axis * 0.15, 0.36 if pony else 0.26)
    if pony:
        hair.ball(poll + axis * 0.38 + UP * 0.18, 0.3)
        hair.ball(poll + axis * 0.55 + UP * 0.12 - side * 0.12, 0.24)
    else:
        blaze = material("Blaze", (0.9, 0.86, 0.8), 0.6)
        for index in range(6):
            t = index / 5.0
            sphere("Blaze%d" % index,
                   poll + axis * ((0.22 + 0.62 * t) * head_len) + UP * (0.28 - 0.1 * t),
                   (0.1, 0.1, 0.12), blaze)


def owlbear():
    fur = material("Fur", (0.23, 0.14, 0.075), 0.85, noise=(8.0, 0.6, (0.11, 0.065, 0.035)))
    disc = material("FacialDisc", (0.55, 0.42, 0.25), 0.9,
                    noise=(12.0, 0.45, (0.38, 0.27, 0.15)))
    beak = material("Beak", (0.07, 0.06, 0.05), 0.35)
    claw = material("Claw", (0.32, 0.30, 0.27), 0.3)
    eye_amber = material("EyeAmber", (1.0, 0.6, 0.1), 0.25,
                         emission=(1.0, 0.5, 0.08), emission_strength=1.2)
    pupil = material("Pupil", (0.015, 0.01, 0.01), 0.2)
    body = Blob("Body", fur, 0.05)
    body.ball((0.0, 0.45, -1.1), 1.75, (1.35, 1.0, 0.85), axis=(1, 0, 0))     # chest
    body.ball((0.0, 0.6, -0.15), 1.15, (1.55, 0.9, 0.7), axis=(1, 0, 0))      # hump
    body.ball((0.0, 0.05, 0.45), 0.78)                                        # neck
    for s in (-1, 1):
        body.ball((s * 1.5, 0.15, -0.3), 0.75)                               # shoulders
        body.chain((s * 1.55, -0.1, -0.5), (s * 1.8, -0.95, -1.05), 0.5, 0.42, 5)
        body.ball((s * 1.8, -1.0, -1.1), 0.5)                                # paw
    head = Blob("Head", fur, 0.04)
    head.ball((0.0, 0.05, 1.3), 1.05, (1.1, 1.0, 1.0), axis=(1, 0, 0))
    head.ball((0.0, -0.3, 1.2), 0.82, (1.15, 0.6, 0.95), axis=(1, 0, 0))
    for s in (-1, 1):
        cone_along("Tuft%d" % s, (s * 0.7, 0.15, 2.0), (s * 0.4, -0.05, 1.0),
                   0.85, 0.15, fur)
        sphere("Disc%d" % s, (s * 0.44, -0.86, 1.3), (0.52, 0.12, 0.55), disc)
        sphere("Eye%d" % s, (s * 0.4, -0.98, 1.34), (0.24, 0.14, 0.24), eye_amber)
        sphere("Pupil%d" % s, (s * 0.4, -1.11, 1.34), (0.1, 0.05, 0.1), pupil)
    cone_along("Beak", (0.0, -0.95, 1.2), (0.0, -0.55, -1.0), 0.62, 0.2, beak, 0.02)
    for s in (-1, 1):
        for index in range(3):
            offset = (index - 1) * 0.2
            cone_along("Claw%d%d" % (s, index),
                       (s * (1.8 + offset * 0.5), -1.25 - abs(offset) * 0.15, -1.2),
                       (s * 0.15, -0.6, -1.0), 0.5, 0.085, claw)








def ape(dire=False):
    """Sprint 18. One builder, two species, because what separates them is
    proportion rather than parts: the gorilla is a dark knuckle-walker with a
    silverback saddle, a crested dome and a short blunt muzzle, while the
    gigantopithecus is a heavier reddish brute whose arms are longer, whose
    muzzle is pushed forward over visible canines and whose hands are clawed.
    Neither is a recolour of the other: the masses, the stance and the head are
    authored twice.

    The body is authored at the size the metaball field was tuned for and the
    whole creature is then scaled as objects, not as numbers. A metaball
    element's `size` expands it in the same units as its radius and its field
    falls off over that radius, so rescaling the authored values pulls merged
    masses apart; scaling the finished objects keeps the surface identical."""
    if dire:
        fur = material("Fur", (0.30, 0.145, 0.065), 0.88,
                       noise=(9.0, 0.62, (0.15, 0.068, 0.028)))
        mane = material("Mane", (0.44, 0.21, 0.085), 0.9,
                        noise=(14.0, 0.5, (0.24, 0.10, 0.042)))
        face = material("Face", (0.19, 0.095, 0.06), 0.6, subsurface=0.08)
        nail = material("Nail", (0.36, 0.34, 0.31), 0.3)
    else:
        fur = material("Fur", (0.095, 0.085, 0.082), 0.9,
                       noise=(9.0, 0.6, (0.038, 0.033, 0.035)))
        mane = material("Saddle", (0.46, 0.46, 0.48), 0.88,
                        noise=(13.0, 0.55, (0.24, 0.24, 0.26)))
        face = material("Face", (0.065, 0.055, 0.055), 0.6, subsurface=0.08)
        nail = material("Nail", (0.24, 0.22, 0.20), 0.3)
    eye = material("Eye", (0.64, 0.42, 0.16), 0.25,
                   emission=(0.72, 0.44, 0.13), emission_strength=0.45)
    pupil = material("Pupil", (0.012, 0.01, 0.01), 0.2)
    tooth = material("Tooth", (0.88, 0.84, 0.74), 0.4)
    mouth = material("MouthDark", (0.025, 0.015, 0.012), 0.6)

    before = set(bpy.context.scene.objects)

    reach = 1.14 if dire else 1.0          # arm length and shoulder span
    bulk = 1.10 if dire else 1.0
    lean = -0.3 if dire else 0.0           # the dire ape pitches forward

    body = Blob("Body", fur, 0.05)
    # Haunch low and back, barrel chest high and forward: the mass of a
    # knuckle-walking primate rather than a bear's level back.
    body.ball((0.0, 0.8, -1.35), 1.5 * bulk, (1.1, 1.0, 0.85), axis=(1, 0, 0))
    body.ball((0.0, 0.25 + lean, -0.3), 1.5 * bulk, (1.45, 1.0, 0.9), axis=(1, 0, 0))
    body.ball((0.0, -0.15 + lean, 0.7), 0.95 * bulk, (1.2, 0.85, 0.5), axis=(1, 0, 0))
    body.ball((0.0, -0.3 + lean, 1.2), 0.78 * bulk, (0.7, 0.6, 0.4), axis=(1, 0, 0))
    body.ball((0.0, -0.4 + lean, 1.55), 0.6 * bulk, (0.5, 0.45, 0.25), axis=(1, 0, 0))
    for s in (-1, 1):
        # Shoulder, upper arm, forearm, and the knuckle the hand turns under.
        # The forearm is the longer segment, which is the primate tell.
        body.ball((s * 1.45 * reach, 0.1 + lean, 0.6), 0.8 * bulk)
        body.chain((s * 1.52 * reach, 0.0 + lean, 0.35),
                   (s * 1.66 * reach, -0.5 + lean, -0.95), 0.55, 0.43, 5)
        body.chain((s * 1.66 * reach, -0.5 + lean, -0.95),
                   (s * 1.6 * reach, -0.82 + lean, -2.05 * reach), 0.43, 0.37, 5)
        # Short bent hind leg folded under the hip.
        body.ball((s * 0.95, 0.6, -1.65), 0.62 * bulk)
        body.chain((s * 0.97, 0.45, -1.9), (s * 1.02, -0.2, -2.3), 0.46, 0.36, 4)

    head = Blob("Head", fur, 0.04)
    # Low dome carried forward of and above the shoulder line so the face
    # reads, with a sagittal crest on top; the dire skull is longer and flatter.
    head.ball((0.0, -0.45 + lean, 1.72), 0.86 * bulk,
              (0.95, 1.15 if dire else 0.9, 0.9), axis=(1, 0, 0))
    head.ball((0.0, -0.35 + lean, 2.26), 0.34 * bulk,
              (0.5, 1.0 if dire else 0.8, 0.22), axis=(1, 0, 0))
    # Heavy brow ridge, then the muzzle: short and blunt on the gorilla,
    # pushed forward with a dropped jaw on the gigantopithecus.
    head.ball((0.0, -1.05 + lean, 1.87), 0.42 * bulk, (1.3, 0.45, 0.3), axis=(1, 0, 0))
    muzzle_y = -1.62 if dire else -1.3
    head.ball((0.0, muzzle_y + lean, 1.47), 0.6 * bulk,
              (0.35, 1.05 if dire else 0.65, 0.55), axis=(0, 1, 0))
    head.ball((0.0, muzzle_y - 0.12 + lean, 1.17), 0.46 * bulk,
              (0.35, 0.65, 0.3), axis=(0, 1, 0))
    for s in (-1, 1):
        head.ball((s * 0.8 * bulk, -0.25 + lean, 1.66), 0.2)

    # The hairless face, and the eyes set deep under the brow.
    sphere("Face", (0.0, muzzle_y + 0.28 + lean, 1.67),
           (0.5, 0.33, 0.54), face, (12 if dire else 6, 0, 0))
    for s in (-1, 1):
        sphere("Eye%d" % s, (s * 0.3, muzzle_y + 0.1 + lean, 1.81),
               (0.12, 0.1, 0.12), eye)
        sphere("Pupil%d" % s, (s * 0.3, muzzle_y - 0.02 + lean, 1.81),
               (0.06, 0.05, 0.06), pupil)
    sphere("Nostrils", (0.0, muzzle_y - 0.32 + lean, 1.49),
           (0.18, 0.075, 0.075), mouth)
    sphere("Mouth", (0.0, muzzle_y - 0.28 + lean, 1.21),
           (0.32, 0.075, 0.1 if dire else 0.05), mouth)
    if dire:
        for s in (-1, 1):
            cone_along("Canine%d" % s, (s * 0.2, muzzle_y - 0.28 + lean, 1.25),
                       (0.0, -0.12, -1.0), 0.28, 0.06, tooth)
            cone_along("LowerCanine%d" % s,
                       (s * 0.17, muzzle_y - 0.28 + lean, 1.13),
                       (0.0, -0.12, 1.0), 0.22, 0.05, tooth)

    for s in (-1, 1):
        # The hand: a closed knuckle the gorilla stands on, a splayed clawed
        # hand on the dire ape.
        hand = (s * 1.6 * reach, -0.86 + lean, -2.1 * reach)
        sphere("Hand%d" % s, hand,
               (0.5, 0.44, 0.34) if dire else (0.44, 0.48, 0.32), fur)
        for index in range(3):
            offset = (index - 1) * 0.22
            if dire:
                cone_along("Claw%d%d" % (s, index),
                           (hand[0] + s * offset, hand[1] - 0.34, hand[2] - 0.08),
                           (s * 0.2, -0.75, -0.6), 0.44, 0.08, nail)
            else:
                sphere("Knuckle%d%d" % (s, index),
                       (hand[0] + s * offset, hand[1] - 0.22, hand[2] - 0.2),
                       (0.14, 0.14, 0.12), fur)
                cone_along("Nail%d%d" % (s, index),
                           (hand[0] + s * offset, hand[1] - 0.3, hand[2] - 0.26),
                           (0.0, -0.3, -1.0), 0.15, 0.06, nail)

    # The back marking is geometry rather than a tint: a silver saddle across
    # the gorilla's lumbar region, a coarse shoulder mane on the dire ape.
    if dire:
        for s in (-1, 1):
            cone_along("Mane%d" % s, (s * 0.72, 0.15 + lean, 0.95),
                       (s * 0.35, 0.95, 0.2), 1.05, 0.34, mane, 0.06)
    else:
        saddle = sphere("Saddle", (0.0, 0.6, -0.25), (1.25, 0.52, 0.85), mane,
                        (8, 0, 0))
        for vertex in saddle.data.vertices:
            if vertex.co.y < 0:
                vertex.co.y *= 0.25

    # Frame it. The creature is authored around three units tall; this is the
    # single place its size and standing height meet the roster framing.
    fit = 0.52 if dire else 0.55
    rise = 0.35
    for obj in set(bpy.context.scene.objects) - before:
        obj.scale = tuple(value * fit for value in obj.scale)
        obj.location = Vector(obj.location) * fit + Vector((0.0, 0.0, rise))


def girallon():
    """Sprint 19. A white four-armed ape, and not the Sprint 18 gorilla with
    two arms added: a girallon stands upright rather than knuckle-walking, so
    its spine is vertical, its haunches are under it rather than behind it, and
    its weight sits on its legs.

    The four arms are the whole silhouette, which sets every proportion here.
    Both pairs are long and deliberately thin, and the two shoulders on a side
    are further apart than their metaball fields are wide - the first pass put
    them 0.80 apart with 0.82 radii and the field fused them into a single
    slab with no arms in it at all. The lower pair hangs further forward so the
    four hands land at four distinct heights at three-quarter view.

    Authored at the size the metaball field was tuned for and scaled as
    objects, not as numbers: a metaball element's field falls off over its
    radius, so rescaling the authored values pulls merged masses apart."""
    fur = material("Fur", (0.82, 0.81, 0.78), 0.92,
                   noise=(10.0, 0.55, (0.56, 0.55, 0.53)))
    shade = material("Shade", (0.50, 0.49, 0.47), 0.9,
                     noise=(16.0, 0.5, (0.32, 0.31, 0.30)))
    face = material("Face", (0.16, 0.11, 0.10), 0.6, subsurface=0.08)
    nail = material("Claw", (0.18, 0.17, 0.16), 0.28)
    eye = material("Eye", (0.82, 0.24, 0.12), 0.25,
                   emission=(0.92, 0.22, 0.08), emission_strength=0.8)
    pupil = material("Pupil", (0.012, 0.01, 0.01), 0.2)
    tooth = material("Tooth", (0.92, 0.89, 0.80), 0.4)
    mouth = material("MouthDark", (0.030, 0.014, 0.012), 0.6)

    before = set(bpy.context.scene.objects)

    body = Blob("Body", fur, 0.05)
    # An upright trunk, narrower than the arm span so the arms carry the
    # outline: hips under the shoulders, a deep chest, a waist drawn in.
    body.ball((0.0, 0.12, -1.50), 1.05, (1.00, 0.95, 0.78), axis=(1, 0, 0))
    body.ball((0.0, 0.02, -0.60), 0.92, (1.00, 0.90, 0.78), axis=(1, 0, 0))
    body.ball((0.0, -0.10, 0.45), 1.12, (1.18, 0.92, 0.85), axis=(1, 0, 0))
    body.ball((0.0, -0.18, 1.25), 0.88, (1.10, 0.84, 0.55), axis=(1, 0, 0))
    body.ball((0.0, -0.22, 1.78), 0.46, (0.48, 0.44, 0.30), axis=(1, 0, 0))

    # Both arm pairs. The two shoulders on a side sit 1.30 apart with 0.50
    # radii, so their fields stay separate and four arms actually read.
    hands = []
    for s in (-1, 1):
        for upper in (True, False):
            shoulder_z = 1.15 if upper else -0.15
            reach = 1.0 if upper else 0.90
            forward = -0.10 if upper else -0.70
            body.ball((s * 1.38, forward * 0.35, shoulder_z), 0.50)
            elbow = (s * 1.80 * reach, forward - 0.30, shoulder_z - 1.25)
            wrist = (s * 1.58 * reach, forward - 0.72,
                     shoulder_z - 2.40 * reach)
            body.chain((s * 1.46, forward * 0.35, shoulder_z - 0.22),
                       elbow, 0.34, 0.27, 6)
            body.chain(elbow, wrist, 0.27, 0.22, 6)
            hands.append((s, wrist))
        # Standing legs, long enough to carry the body rather than nubs.
        body.ball((s * 0.68, 0.10, -1.95), 0.56)
        body.chain((s * 0.70, 0.05, -2.20), (s * 0.78, -0.05, -3.35),
                   0.46, 0.34, 6)
        body.ball((s * 0.80, -0.38, -3.48), 0.30, (0.62, 1.15, 0.38),
                  axis=(0, 1, 0))

    head = Blob("Head", fur, 0.04)
    # A broad flat skull, carried high and forward so it reads at icon size.
    # The first pass made it a third of this and the creature read as a toy.
    head.ball((0.0, -0.34, 2.14), 1.00, (1.02, 0.92, 0.80), axis=(1, 0, 0))
    # A low sagittal crest, and a brow heavy enough that the eyes sit under
    # it rather than on the front of a round skull.
    head.ball((0.0, -0.26, 2.62), 0.34, (0.46, 0.86, 0.20), axis=(1, 0, 0))
    head.ball((0.0, -1.10, 2.26), 0.52, (1.34, 0.40, 0.30), axis=(1, 0, 0))
    head.ball((0.0, -1.56, 1.84), 0.64, (0.40, 1.05, 0.50), axis=(0, 1, 0))
    head.ball((0.0, -1.70, 1.50), 0.48, (0.36, 0.72, 0.28), axis=(0, 1, 0))
    for s in (-1, 1):
        # Small ears flat against the skull, not round tabs beside it.
        head.ball((s * 0.86, -0.18, 2.06), 0.17, (0.5, 0.9, 1.0),
                  axis=(1, 0, 0))

    sphere("Face", (0.0, -1.24, 1.98), (0.54, 0.38, 0.56), face, (10, 0, 0))
    for s in (-1, 1):
        sphere("Eye%d" % s, (s * 0.30, -1.44, 2.12), (0.12, 0.10, 0.11), eye)
        sphere("Pupil%d" % s, (s * 0.30, -1.54, 2.12),
               (0.06, 0.05, 0.055), pupil)
    sphere("Nostrils", (0.0, -1.96, 1.82), (0.18, 0.08, 0.07), mouth)
    sphere("Mouth", (0.0, -1.92, 1.52), (0.34, 0.08, 0.11), mouth)
    # Four canines rather than two: a girallon is a predator, and the open
    # mouth is most of what separates it from an ape at icon size.
    for s in (-1, 1):
        cone_along("Canine%d" % s, (s * 0.22, -1.90, 1.58),
                   (0.0, -0.16, -1.0), 0.32, 0.07, tooth)
        cone_along("LowerCanine%d" % s, (s * 0.18, -1.88, 1.44),
                   (0.0, -0.16, 1.0), 0.24, 0.055, tooth)

    # Four clawed hands, three claws each. The claws are the printed attack,
    # so they are geometry and not a tint.
    for index, (s, wrist) in enumerate(hands):
        hand = (wrist[0], wrist[1] - 0.16, wrist[2] - 0.20)
        sphere("Hand%d" % index, hand, (0.36, 0.34, 0.26), fur)
        for digit in range(3):
            offset = (digit - 1) * 0.18
            cone_along("Claw%d%d" % (index, digit),
                       (hand[0] + s * offset, hand[1] - 0.22, hand[2] - 0.08),
                       (s * 0.18, -0.70, -0.68), 0.40, 0.065, nail)

    # A grey mantle across the upper shoulders, as geometry rather than a
    # tint, so a nearly white creature still has a silhouette against a pale
    # rim. It sits above the upper arm sockets and clear of the lower pair.
    for s in (-1, 1):
        cone_along("Mantle%d" % s, (s * 0.62, -0.06, 1.34),
                   (s * 0.42, 0.88, 0.08), 0.88, 0.26, shade, 0.05)

    # Frame it. The creature stands about seven authored units from sole to
    # crest, centred near -0.3, so this is the single place its size and
    # standing height meet the roster framing. The second pass left it a
    # half unit above the camera target with empty space beneath it.
    fit = 0.44
    rise = 0.18
    for obj in set(bpy.context.scene.objects) - before:
        obj.scale = tuple(value * fit for value in obj.scale)
        obj.location = Vector(obj.location) * fit + Vector((0.0, 0.0, rise))

def xill():
    """Sprint 19. A chitinous four-armed outsider: not a primate at all, so
    nothing here is borrowed from the Girallon. The trunk is a segmented
    insect thorax over a narrow abdomen, the limbs are thin and jointed rather
    than muscled, the head is a wedge on a real neck with mandibles and four
    compound eyes, and all four hands end in hooked claws.

    The first pass sank the head into the shoulders, merged the four eyes into
    one green mass and authored the thoracic plates inside the trunk where
    nothing could see them. The head is larger and lifted on a neck now, the
    eyes are smaller and spaced, and the plates sit proud of the chest."""
    chitin = material("Chitin", (0.52, 0.17, 0.09), 0.38,
                      noise=(12.0, 0.4, (0.34, 0.10, 0.05)))
    plate = material("Plate", (0.70, 0.28, 0.12), 0.28,
                     noise=(18.0, 0.35, (0.46, 0.17, 0.07)))
    joint = material("Joint", (0.20, 0.07, 0.04), 0.55)
    nail = material("Claw", (0.13, 0.11, 0.10), 0.25)
    eye = material("Eye", (0.05, 0.42, 0.16), 0.2,
                   emission=(0.06, 0.52, 0.18), emission_strength=0.6)
    mouth = material("Maw", (0.030, 0.012, 0.010), 0.6)

    before = set(bpy.context.scene.objects)

    body = Blob("Body", chitin, 0.045)
    # Narrow hips, a tall thorax that widens where the arm pairs anchor, and
    # a neck that actually lifts the head clear of the shoulders.
    body.ball((0.0, 0.10, -1.45), 0.82, (0.82, 0.80, 0.72), axis=(1, 0, 0))
    body.ball((0.0, 0.00, -0.70), 0.80, (0.80, 0.78, 0.68), axis=(1, 0, 0))
    body.ball((0.0, -0.12, 0.15), 1.02, (1.06, 0.82, 0.68), axis=(1, 0, 0))
    body.ball((0.0, -0.18, 0.95), 0.96, (1.10, 0.80, 0.60), axis=(1, 0, 0))
    # A neck that actually bridges thorax and head. The second pass left a
    # visible gap and the head read as a separate object floating above the
    # shoulders.
    body.ball((0.0, -0.24, 1.52), 0.40, (0.46, 0.46, 0.46), axis=(1, 0, 0))
    body.chain((0.0, -0.26, 1.58), (0.0, -0.34, 2.00), 0.34, 0.30, 4)

    # Two arm pairs, 1.24 apart with 0.40 radii so their fields stay clear.
    hands = []
    for s in (-1, 1):
        for upper in (True, False):
            shoulder_z = 1.02 if upper else -0.22
            reach = 1.0 if upper else 0.92
            forward = -0.20 if upper else -0.66
            body.ball((s * 1.12, forward * 0.25, shoulder_z), 0.40)
            elbow = (s * 1.68 * reach, forward - 0.46, shoulder_z - 0.92)
            wrist = (s * 1.26 * reach, forward - 0.92,
                     shoulder_z - 2.02 * reach)
            body.chain((s * 1.20, forward * 0.25, shoulder_z - 0.14),
                       elbow, 0.26, 0.19, 6)
            body.chain(elbow, wrist, 0.19, 0.15, 6)
            sphere("Elbow%d%d" % (s, 1 if upper else 0), elbow,
                   (0.17, 0.17, 0.17), joint)
            hands.append((s, wrist))
        # Digitigrade legs: a long thigh, a reversed shin, a narrow foot.
        body.ball((s * 0.58, 0.08, -1.72), 0.44)
        knee = (s * 0.72, 0.40, -2.52)
        body.chain((s * 0.60, 0.05, -1.92), knee, 0.34, 0.24, 6)
        body.chain(knee, (s * 0.70, -0.34, -3.32), 0.24, 0.16, 6)
        sphere("Knee%d" % s, knee, (0.22, 0.22, 0.22), joint)
        sphere("Foot%d" % s, (s * 0.72, -0.62, -3.40), (0.18, 0.56, 0.14),
               chitin)

    head = Blob("Head", plate, 0.035)
    # A forward wedge carried on the neck, large enough to read at icon size
    # and low enough to sit on the shoulders rather than hover over them.
    head.ball((0.0, -0.44, 1.98), 0.80, (0.88, 0.98, 0.62), axis=(1, 0, 0))
    head.ball((0.0, -1.16, 1.80), 0.56, (0.60, 0.92, 0.42), axis=(0, 1, 0))
    head.ball((0.0, -0.16, 2.16), 0.42, (0.78, 0.54, 0.30), axis=(1, 0, 0))

    # Four compound eyes: a large pair forward and a small pair above and
    # outboard, spaced so they read as four and not as one mass.
    # Four compound eyes on the FRONT of the wedge, where a face is, rather
    # than on the sides of the skull where the second pass put them and they
    # read as two pale blobs stuck on.
    for s in (-1, 1):
        sphere("EyeBig%d" % s, (s * 0.26, -1.30, 1.90),
               (0.14, 0.12, 0.13), eye)
        sphere("EyeSmall%d" % s, (s * 0.40, -1.06, 2.10),
               (0.085, 0.08, 0.085), eye)
    sphere("Maw", (0.0, -1.46, 1.68), (0.20, 0.10, 0.12), mouth)
    # Mandibles: the printed bite, so they are geometry.
    for s in (-1, 1):
        cone_along("Mandible%d" % s, (s * 0.30, -1.28, 1.74),
                   (-s * 0.50, -1.0, -0.30), 0.66, 0.08, plate, 0.02)
        cone_along("Palp%d" % s, (s * 0.16, -1.42, 1.54),
                   (-s * 0.2, -0.7, -0.8), 0.32, 0.045, joint)

    # Four hooked claws, two hooks each: thin and curved rather than the
    # Girallon's broad nails, because nothing about these two is shared.
    for index, (s, wrist) in enumerate(hands):
        hand = (wrist[0], wrist[1] - 0.12, wrist[2] - 0.14)
        sphere("Hand%d" % index, hand, (0.18, 0.21, 0.15), chitin)
        for digit in range(2):
            offset = (digit - 0.5) * 0.18
            cone_along("Claw%d%d" % (index, digit),
                       (hand[0] + s * offset, hand[1] - 0.16, hand[2] - 0.05),
                       (s * 0.3, -0.80, -0.55), 0.38, 0.05, nail)

    # A segmented abdomen, as rings that hug the trunk below both arm
    # sockets. The second pass laid three boxes on the chest and they read as
    # rectangles stuck on; the third made the rings wider than the body and
    # they read as hoops hanging off it and cutting across the arms. These
    # sit between the lower shoulders and the hips, where the trunk is
    # narrow and nothing else is in the way.
    for row in range(3):
        torus("Segment%d" % row, (0.0, -0.04 + row * 0.03, -0.58 - row * 0.40),
              0.60 - row * 0.03, 0.065, plate)

    # Frame it. The creature stands about six authored units from sole to
    # crown, centred near -0.45, so this is the single place its size and
    # standing height meet the roster framing.
    fit = 0.47
    rise = 0.24
    for obj in set(bpy.context.scene.objects) - before:
        obj.scale = tuple(value * fit for value in obj.scale)
        obj.location = Vector(obj.location) * fit + Vector((0.0, 0.0, rise))

def cyclops():
    skin = material("Skin", (0.5, 0.33, 0.2), 0.7, subsurface=0.12,
                    noise=(7.0, 0.5, (0.33, 0.19, 0.11)))
    hide = material("Hide", (0.25, 0.155, 0.08), 0.85, noise=(10.0, 0.5, (0.14, 0.085, 0.045)))
    strap = material("Strap", (0.1, 0.07, 0.045), 0.7)
    steel = material("Steel", (0.6, 0.62, 0.64), 0.3, metallic=1.0)
    haft = material("Haft", (0.15, 0.095, 0.05), 0.65, noise=(20.0, 0.5, (0.07, 0.045, 0.025)))
    eye_white = material("EyeWhite", (0.88, 0.84, 0.76), 0.3)
    iris = material("Iris", (0.85, 0.42, 0.08), 0.25, emission=(0.9, 0.38, 0.05),
                    emission_strength=0.5)
    pupil = material("Pupil", (0.015, 0.01, 0.01), 0.2)
    tusk = material("Tusk", (0.82, 0.77, 0.65), 0.45)
    body = Blob("Body", skin, 0.05)
    body.ball((0.0, 0.4, -1.1), 1.9, (1.5, 1.0, 0.8), axis=(1, 0, 0))          # chest
    body.ball((0.0, 0.05, 0.15), 0.72, (1.1, 1.0, 0.7), axis=(1, 0, 0))        # neck
    for s in (-1, 1):
        body.ball((s * 1.6, 0.2, -0.45), 0.9)                                 # shoulders
    body.chain((1.85, -0.2, -0.7), (2.05, -0.65, -1.5), 0.5, 0.42, 5)           # right arm
    body.ball((2.05, -0.7, -0.6), 0.46)                                        # fist on the haft
    head = Blob("Head", skin, 0.04)
    head.ball((0.0, 0.0, 1.15), 1.05, (1.0, 1.05, 1.15), axis=(1, 0, 0))
    head.ball((0.0, -0.2, 0.62), 0.78, (1.05, 0.9, 0.75), axis=(1, 0, 0))     # jaw
    head.ball((0.0, -0.42, 1.48), 0.55, (1.6, 0.6, 0.42), axis=(1, 0, 0))     # brow
    for s in (-1, 1):
        cone_along("Ear%d" % s, (s * 0.85, 0.1, 1.3), (s * 1.0, 0.05, 0.3), 0.62, 0.18, skin)
    sphere("Eye", (0.0, -0.82, 1.2), (0.32, 0.22, 0.32), eye_white)
    sphere("Iris", (0.0, -1.0, 1.2), (0.17, 0.09, 0.17), iris)
    sphere("Pupil", (0.0, -1.08, 1.2), (0.075, 0.04, 0.075), pupil)
    sphere("Mouth", (0.0, -0.88, 0.58), (0.34, 0.07, 0.045),
           material("MouthDark", (0.03, 0.02, 0.015), 0.6))
    for s in (-1, 1):
        cone_along("Tusk%d" % s, (s * 0.26, -0.8, 0.5), (0.0, -0.15, 1.0), 0.32, 0.065, tusk)
    for s in (-1, 1):
        sphere("Pauldron%d" % s, (s * 1.6, 0.05, -0.15), (0.72, 0.6, 0.45), hide)
    box("Strap", (0.1, -1.25, -0.85), (0.3, 0.06, 2.6), strap, (0, 34, 0), bevel=0.02)
    cylinder("Haft", (1.85, -0.75, 0.35), 0.08, 3.5, haft, (0, -5, 0))
    # a single-bladed axe head flaring out from its socket to a wide edge
    blade = box("Blade", (2.2, -0.75, 1.9), (0.85, 0.1, 0.7), steel, (0, -5, 0),
                bevel=0.03)
    for vertex in blade.data.vertices:
        if vertex.co.x > 0:
            vertex.co.z *= 1.6
            vertex.co.y *= 0.35
    box("Socket", (1.8, -0.75, 1.9), (0.34, 0.22, 0.56), steel, (0, -5, 0), bevel=0.03)



def torus(name, location, major, minor, mat, rotation=(0, 0, 0)):
    bpy.ops.mesh.primitive_torus_add(major_radius=major, minor_radius=minor,
                                     major_segments=64, minor_segments=24,
                                     location=location)
    obj = bpy.context.active_object
    obj.name = name
    obj.rotation_euler = Euler([math.radians(v) for v in rotation], "XYZ")
    smooth(obj, 0)
    assign(obj, mat)
    return obj


def shambling_mound():
    """A hulking mound of rotting vegetation rearing up, two heavy slam limbs
    raised, a hollow dark face with a faint marsh glow, vines and leaves
    hanging off the mass."""
    moss = material("Moss", (0.13, 0.21, 0.07), 0.92, noise=(5.0, 0.8, (0.05, 0.08, 0.025)))
    rot = material("Rot", (0.24, 0.17, 0.07), 0.95, noise=(9.0, 0.55, (0.1, 0.07, 0.03)))
    leaf = material("Leaf", (0.26, 0.44, 0.12), 0.7, noise=(14.0, 0.4, (0.12, 0.22, 0.06)))
    vine = material("Vine", (0.13, 0.17, 0.06), 0.85)
    hollow = material("Hollow", (0.02, 0.025, 0.015), 0.9)
    glow = material("Glow", (0.55, 0.85, 0.35), 0.3, emission=(0.45, 0.8, 0.25),
                    emission_strength=1.6)
    body = Blob("Body", moss, 0.05)
    body.ball((0.0, 0.6, -1.05), 2.0, (1.5, 1.05, 0.8), axis=(1, 0, 0))      # base mass
    body.ball((0.0, 0.3, 0.15), 1.35, (1.25, 1.0, 0.9), axis=(1, 0, 0))      # upper mass
    body.ball((0.0, 0.0, 1.05), 0.95)                                        # crown
    for s in (-1, 1):
        body.ball((s * 1.55, 0.2, 0.05), 0.85)                               # shoulders
        body.chain((s * 1.7, -0.1, 0.1), (s * 2.15, -0.9, 1.15), 0.55, 0.62, 6)
        body.ball((s * 2.2, -0.95, 1.25), 0.6, (1.0, 1.15, 0.9), axis=(1, 0, 0))
    decay = Blob("Rot", rot, 0.05)
    decay.ball((0.35, -0.55, -0.5), 0.85, (1.3, 0.8, 0.9), axis=(1, 0, 0))
    decay.ball((-0.6, -0.4, -1.2), 0.9)
    decay.ball((0.2, -0.3, 0.55), 0.55)
    decay.ball((-1.3, -0.5, 0.3), 0.5)
    for s in (-1, 1):
        sphere("Socket%d" % s, (s * 0.34, -0.84, 1.12), (0.15, 0.1, 0.13), hollow)
        sphere("Ember%d" % s, (s * 0.34, -0.9, 1.12), (0.05, 0.03, 0.05), glow)
    sphere("Mouth", (0.0, -0.86, 0.7), (0.3, 0.08, 0.1), hollow)
    # small separate leaves scattered over the front of the mass, clear of the
    # face, and vines hanging from the raised limbs
    for index in range(34):
        a = index * 2.399
        r = 0.5 + 1.6 * ((index * 37) % 10) / 10.0
        x = r * math.cos(a) * 1.15
        z = -1.5 + 2.7 * ((index * 53) % 10) / 10.0
        if abs(x) < 0.7 and z > 0.45:
            continue
        y = -0.9 - 0.3 * max(0.0, 1.0 - abs(x) / 2.2) + 0.2 * abs(z)
        sphere("Foliage%d" % index, (x, y, z),
               (0.22 + 0.08 * ((index * 7) % 3), 0.14, 0.05), leaf,
               (30 * math.sin(a * 1.3), 20 * math.cos(a), math.degrees(a)))
    for s in (-1, 1):
        for index in range(4):
            start = (s * (1.7 + 0.25 * index), -0.85 - 0.1 * index, 1.0 - 0.18 * index)
            cone_along("ArmVine%d%d" % (s, index), start, (s * 0.1, -0.15, -1.0),
                       1.4 + 0.5 * ((index * 5) % 3), 0.06, vine, 0.015)
    for index in range(14):
        a = index * 0.45 + 0.2
        x = 1.9 * math.cos(a)
        y = max(-0.1, 0.55 + 0.9 * math.sin(a))
        cone_along("Vine%d" % index, (x * 0.9, y * 0.7 - 0.3, -0.2 - 0.05 * index),
                   (0.15 * math.cos(a), -0.3, -1.0),
                   1.3 + 0.35 * ((index * 7) % 3), 0.07, vine, 0.02)
    for index in range(10):
        a = index * 0.63
        sphere("Leaf%d" % index,
               (1.6 * math.cos(a), 0.2 + 0.6 * math.sin(a), 0.4 + 0.5 * math.sin(a * 1.7)),
               (0.34, 0.22, 0.05), leaf, (35 * math.sin(a), 0, math.degrees(a)))


def giant_flytrap():
    """A huge flytrap from the front: the main trap gaping at the viewer, red
    inside and rimmed with pale spines, two smaller traps behind it on thick
    stalks, tendrils curling out of the base."""
    stalk = material("Stalk", (0.2, 0.36, 0.1), 0.6, noise=(8.0, 0.45, (0.1, 0.2, 0.05)))
    lobe = material("Lobe", (0.28, 0.5, 0.14), 0.55, subsurface=0.2,
                    noise=(10.0, 0.4, (0.16, 0.3, 0.08)))
    inner = material("Inner", (0.72, 0.12, 0.1), 0.45, subsurface=0.3,
                     emission=(0.5, 0.06, 0.04), emission_strength=0.25)
    spine = material("Spine", (0.85, 0.82, 0.55), 0.5)
    tendril = material("Tendril", (0.15, 0.28, 0.08), 0.75)
    base = Blob("Base", stalk, 0.05)
    base.ball((0.0, 0.8, -1.9), 1.9, (1.6, 1.0, 0.6), axis=(1, 0, 0))
    base.ball((0.9, 0.3, -1.6), 0.9)
    base.ball((-1.1, 0.5, -1.7), 0.85)

    def trap(name, hinge, direction, length, width, gape, spines):
        """Two lobes hinged at `hinge`, opening toward `direction` by `gape`
        degrees each; the inside is red and the far rim carries spines."""
        direction = Vector(direction).normalized()
        side = direction.cross(UP).normalized()
        up = side.cross(direction).normalized()
        for sign, tag in ((1, "Upper"), (-1, "Lower")):
            tilt = Euler((0.0, 0.0, 0.0))
            q = mathutils.Quaternion(side, math.radians(sign * gape))
            axis = q @ direction
            normal = q @ up
            centre = Vector(hinge) + axis * (length * 0.5)
            rotation = axis.to_track_quat("Y", "Z")
            # "Y" tracks the lobe length; roll so local Z follows the normal
            euler = rotation.to_euler()
            obj = sphere(name + tag, centre, (width, length * 0.55, 0.18), lobe)
            obj.rotation_euler = euler
            lining = sphere(name + tag + "Inner", centre - normal * (sign * 0.13),
                            (width * 0.9, length * 0.5, 0.07), inner)
            lining.rotation_euler = euler
            for k in range(spines):
                t = (k + 0.5) / spines
                arc = (t - 0.5) * math.pi
                rim = Vector(hinge) + axis * (length * (0.55 + 0.45 * math.cos(arc))) + \
                    side * (width * 0.95 * math.sin(arc))
                point = (-normal * sign * 0.7 + axis * 0.35).normalized()
                cone_along(name + tag + "Spine%d" % k, rim, point, 0.42, 0.045, spine)

    cylinder("MainStalk", (0.0, 0.2, -0.7), 0.42, 2.3, stalk, (12, 0, 0))
    trap("Main", (0.0, -0.25, 0.55), (0.0, -0.75, 0.45), 2.3, 1.35, 34, 9)
    cylinder("LeftStalk", (-1.6, 0.9, -0.6), 0.28, 2.4, stalk, (10, 0, 35))
    trap("Left", (-2.2, 0.35, 0.7), (-0.55, -0.6, 0.5), 1.5, 0.85, 30, 7)
    cylinder("RightStalk", (1.7, 1.0, -0.5), 0.28, 2.5, stalk, (10, 0, -35))
    trap("Right", (2.35, 0.4, 0.85), (0.55, -0.6, 0.45), 1.5, 0.85, 30, 7)
    for index in range(8):
        a = index * 0.8 + 0.3
        start = (1.9 * math.cos(a), 0.7 + 0.5 * math.sin(a), -1.4)
        cone_along("Tendril%d" % index, start,
                   (0.6 * math.cos(a), -0.4, 0.9 + 0.3 * math.sin(a * 2.0)),
                   1.1 + 0.4 * ((index * 5) % 3), 0.06, tendril, 0.015)


def purple_worm():
    """A gargantuan worm rearing out of the lower right, its segmented purple
    body curving up to a round maw that faces the viewer, ringed with lips and
    two circles of teeth around a black throat."""
    skin = material("Skin", (0.36, 0.14, 0.42), 0.55, subsurface=0.15,
                    noise=(7.0, 0.5, (0.2, 0.07, 0.25)))
    band = material("Band", (0.2, 0.07, 0.26), 0.7, noise=(9.0, 0.4, (0.12, 0.04, 0.16)))
    lip = material("Lip", (0.5, 0.18, 0.5), 0.5, subsurface=0.2)
    gum = material("Gum", (0.36, 0.06, 0.12), 0.5, subsurface=0.2)
    throat = material("Throat", (0.04, 0.01, 0.025), 0.85)
    tooth = material("Tooth", (0.9, 0.86, 0.72), 0.4)
    path = [Vector(v) for v in ((3.0, 1.6, -2.6), (2.35, 1.2, -1.5), (1.55, 0.85, -0.55),
                                (0.85, 0.55, 0.2), (0.35, 0.45, 0.65))]
    radii = (1.05, 1.18, 1.22, 1.15, 1.05)
    body = Blob("Body", skin, 0.05)
    for index in range(len(path) - 1):
        body.chain(path[index], path[index + 1], radii[index], radii[index + 1], 6)
    # darker bands ring the body every so often along the curve
    for index in range(1, 12):
        t = index / 12.0
        seg = min(int(t * (len(path) - 1)), len(path) - 2)
        local = t * (len(path) - 1) - seg
        centre = path[seg].lerp(path[seg + 1], local)
        axis = (path[seg + 1] - path[seg]).normalized()
        radius = radii[seg] + (radii[seg + 1] - radii[seg]) * local
        ring = torus("Band%d" % index, centre, radius * 0.98, 0.09, band)
        ring.rotation_euler = axis.to_track_quat("Z", "Y").to_euler()
    head = Vector((0.05, -0.55, 1.0))
    facing = Vector((0.0, -1.0, 0.3)).normalized()
    u = facing.cross(UP).normalized()
    v = u.cross(facing).normalized()
    body.ball(head - facing * 1.15, 1.0)
    lips = torus("Lips", head, 0.95, 0.34, lip)
    lips.rotation_euler = facing.to_track_quat("Z", "Y").to_euler()
    gums = torus("Gums", head + facing * 0.06, 0.66, 0.15, gum)
    gums.rotation_euler = facing.to_track_quat("Z", "Y").to_euler()
    hole = sphere("Throat", head - facing * 0.3, (0.7, 0.7, 0.45), throat)
    hole.rotation_euler = facing.to_track_quat("Z", "Y").to_euler()
    for ring_index, (radius, count, length) in enumerate(((0.8, 16, 0.5), (0.52, 10, 0.36))):
        for k in range(count):
            a = k * 2.0 * math.pi / count + ring_index * 0.2
            point = head + facing * (0.12 - ring_index * 0.14) + \
                (u * math.cos(a) + v * math.sin(a)) * radius
            inward = ((-(u * math.cos(a) + v * math.sin(a))) * 0.7 + facing * 0.3).normalized()
            cone_along("Tooth%d_%d" % (ring_index, k), point, inward, length + 0.08, 0.09, tooth)
    # a stinger tail tip rising behind the body on the left
    cone_along("Tail", (-1.6, 1.4, -1.9), (-0.5, -0.2, 1.0), 1.7, 0.42, skin, 0.05)
    cone_along("Stinger", (-2.3, 1.1, -0.45), (-0.5, -0.2, 1.0), 0.6, 0.12, tooth)



MEPHIT_DRESSINGS = {
    "dust": dict(skin=(0.55, 0.45, 0.28), dark=(0.3, 0.24, 0.13), wing=(0.42, 0.34, 0.2),
                 eye=(0.95, 0.85, 0.5), breath=(0.7, 0.6, 0.35), motes=(0.75, 0.66, 0.4),
                 crust=None, glow=None, drip=None, crystal=None,
                 inner=(0.32, 0.26, 0.14), outer=(0.03, 0.025, 0.015),
                 key=(1.0, 0.9, 0.7), rim=(0.9, 0.8, 0.6)),
    "ice": dict(skin=(0.6, 0.75, 0.9), dark=(0.25, 0.4, 0.6), wing=(0.5, 0.65, 0.85),
                eye=(0.6, 0.9, 1.0), breath=(0.75, 0.9, 1.0), motes=(0.9, 0.97, 1.0),
                crust=None, glow=None, drip=None, crystal=(0.85, 0.95, 1.0),
                inner=(0.12, 0.22, 0.34), outer=(0.01, 0.015, 0.03),
                key=(0.85, 0.92, 1.0), rim=(0.6, 0.8, 1.0)),
    "magma": dict(skin=(0.12, 0.06, 0.05), dark=(0.06, 0.03, 0.03), wing=(0.16, 0.07, 0.05),
                  eye=(1.0, 0.7, 0.2), breath=(1.0, 0.45, 0.1), motes=(1.0, 0.55, 0.15),
                  crust=(0.09, 0.05, 0.04), glow=(1.0, 0.35, 0.05), drip=None, crystal=None,
                  inner=(0.3, 0.1, 0.04), outer=(0.03, 0.01, 0.005),
                  key=(1.0, 0.75, 0.5), rim=(1.0, 0.5, 0.3)),
    "ooze": dict(skin=(0.3, 0.42, 0.14), dark=(0.15, 0.24, 0.07), wing=(0.24, 0.34, 0.11),
                 eye=(0.85, 0.95, 0.3), breath=(0.55, 0.75, 0.2), motes=(0.6, 0.8, 0.25),
                 crust=None, glow=None, drip=(0.45, 0.6, 0.18), crystal=None,
                 inner=(0.12, 0.2, 0.07), outer=(0.012, 0.02, 0.008),
                 key=(0.95, 0.95, 0.75), rim=(0.6, 0.9, 0.5)),
    "salt": dict(skin=(0.85, 0.84, 0.8), dark=(0.55, 0.54, 0.5), wing=(0.75, 0.74, 0.7),
                 eye=(0.95, 0.9, 0.75), breath=(0.9, 0.9, 0.88), motes=(1.0, 1.0, 0.98),
                 crust=None, glow=None, drip=None, crystal=(0.95, 0.95, 0.92),
                 inner=(0.3, 0.3, 0.28), outer=(0.03, 0.03, 0.028),
                 key=(1.0, 0.98, 0.9), rim=(0.8, 0.85, 0.95)),
    "steam": dict(skin=(0.7, 0.72, 0.75), dark=(0.4, 0.42, 0.46), wing=(0.6, 0.62, 0.66),
                  eye=(1.0, 0.85, 0.6), breath=(0.9, 0.9, 0.9), motes=(0.95, 0.95, 0.95),
                  crust=None, glow=(0.9, 0.5, 0.3), drip=None, crystal=None,
                  inner=(0.25, 0.27, 0.3), outer=(0.02, 0.022, 0.026),
                  key=(1.0, 0.9, 0.8), rim=(0.7, 0.8, 0.95)),
}


def mephit(element):
    """A small, sharp-featured winged mephit bust facing the viewer with its
    mouth open mid-breath, dressed for its element."""
    d = MEPHIT_DRESSINGS[element]
    skin = material("Skin", d["skin"], 0.6, subsurface=0.1, noise=(9.0, 0.5, d["dark"]))
    wing = material("Wing", d["wing"], 0.7, noise=(6.0, 0.35, d["dark"]))
    horn = material("Horn", d["dark"], 0.45)
    eye = material("Eye", d["eye"], 0.2, emission=d["eye"], emission_strength=1.4)
    mouth = material("Mouth", (0.04, 0.02, 0.02), 0.8)
    breath = material("Breath", d["breath"], 0.9, emission=d["breath"], emission_strength=0.35)
    motes = material("Motes", d["motes"], 0.5, emission=d["motes"], emission_strength=0.9)
    body = Blob("Body", skin, 0.045)
    body.ball((0.0, 0.5, -1.2), 1.15, (1.1, 0.9, 1.0), axis=(1, 0, 0))     # chest
    body.ball((0.0, 0.3, -0.35), 0.62)                                       # neck
    body.ball((0.0, 0.0, 0.55), 0.92, (1.0, 0.95, 1.05), axis=(1, 0, 0))    # head
    body.ball((0.0, -0.35, 0.15), 0.62, (1.1, 0.8, 0.7), axis=(1, 0, 0))    # jaw
    for s in (-1, 1):
        body.ball((s * 0.95, 0.35, -0.75), 0.48)                             # shoulders
        body.chain((s * 1.0, 0.1, -0.85), (s * 1.35, -0.75, -1.35), 0.32, 0.26, 5)  # arms
        body.ball((s * 1.4, -0.85, -1.4), 0.3)                               # hands
        cone_along("Horn%d" % s, (s * 0.5, 0.1, 1.2), (s * 0.55, 0.1, 1.0), 0.75, 0.16, horn, 0.02)
        cone_along("Ear%d" % s, (s * 0.85, 0.15, 0.55), (s * 1.0, 0.1, 0.45), 0.7, 0.17, skin)
        sphere("Eye%d" % s, (s * 0.36, -0.78, 0.7), (0.17, 0.1, 0.17), eye)
        # a large bat wing rising behind each shoulder: a fan of spars and a membrane
        for k in range(4):
            angle = 25 + k * 22
            cone_along("Spar%d%d" % (s, k), (s * 1.0, 0.85, -0.55),
                       (s * math.cos(math.radians(angle)), 0.2, math.sin(math.radians(angle))),
                       2.6 - 0.25 * k, 0.07, wing, 0.015)
        membrane = sphere("Membrane%d" % s, (s * 2.05, 0.95, 0.55), (1.35, 0.06, 1.25), wing,
                          (0, 0, s * -25))
    sphere("MouthHole", (0.0, -0.9, 0.12), (0.32, 0.12, 0.18), mouth)
    for k in range(6):
        cone_along("Fang%d" % k, (-0.25 + 0.1 * k, -0.92, 0.28 if k % 2 == 0 else -0.02),
                   (0.0, -0.1, -1.0 if k % 2 == 0 else 1.0), 0.14, 0.03, horn)
    # the breath: a cone of motes leaving the mouth toward the lower left
    for k in range(18):
        t = k / 17.0
        spread = 0.15 + 0.9 * t
        x = -0.15 - 2.2 * t + spread * math.sin(k * 2.1) * 0.35
        z = 0.0 - 0.9 * t + spread * math.cos(k * 1.7) * 0.35
        y = -1.1 - 0.6 * t
        sphere("Mote%d" % k, (x, y, z), (0.05 + 0.07 * t,) * 3, motes if k % 3 else breath)
    if d["crust"]:
        crust = material("Crust", d["crust"], 0.95, noise=(12.0, 0.5, (0.02, 0.01, 0.01)))
        glow = material("Glow", d["glow"], 0.4, emission=d["glow"], emission_strength=2.5)
        for k in range(12):
            a = k * 2.399
            sphere("Crust%d" % k, (1.0 * math.cos(a), -0.6 + 0.3 * math.sin(a * 0.7),
                                    -0.4 + 0.9 * math.sin(a)),
                   (0.28, 0.12, 0.2), crust, (20 * math.sin(a), 0, math.degrees(a)))
            sphere("Ember%d" % k, (0.95 * math.cos(a + 0.3), -0.75, -0.3 + 0.8 * math.sin(a + 0.3)),
                   (0.05, 0.03, 0.05), glow)
    if d["glow"] and not d["crust"]:
        glow = material("Glow", d["glow"], 0.4, emission=d["glow"], emission_strength=1.2)
        sphere("Core", (0.0, -0.3, -1.05), (0.45, 0.2, 0.5), glow)
    if d["crystal"]:
        crystal = material("Crystal", d["crystal"], 0.15, emission=d["crystal"], emission_strength=0.2)
        for k in range(10):
            a = k * 2.399
            cone_along("Crystal%d" % k, (0.9 * math.cos(a), -0.35 + 0.3 * math.sin(a * 0.5),
                                         -0.7 + 0.9 * math.sin(a)),
                       (0.6 * math.cos(a), -0.5, 0.4 + 0.5 * math.sin(a)),
                       0.35 + 0.2 * ((k * 5) % 3), 0.07, crystal, 0.0)
    if d["drip"]:
        drip = material("Drip", d["drip"], 0.2, subsurface=0.3)
        for k in range(9):
            a = k * 2.399
            cone_along("Drip%d" % k, (1.0 * math.cos(a), -0.55, -0.6 + 0.7 * math.sin(a)),
                       (0.0, 0.0, -1.0), 0.5 + 0.3 * ((k * 7) % 3), 0.07, drip, 0.02)


def tiger():
    """A tiger's head and shoulders facing the viewer: orange coat with dark
    stripes over the brow and cheeks, white muzzle and chin, round ears,
    amber eyes, a snarl of fangs."""
    orange = material("Orange", (0.82, 0.42, 0.10), 0.7, noise=(7.0, 0.35, (0.55, 0.25, 0.05)))
    dark = material("Stripe", (0.08, 0.05, 0.04), 0.8)
    white = material("White", (0.95, 0.92, 0.85), 0.6, subsurface=0.15)
    eye = material("Eye", (0.95, 0.65, 0.15), 0.2, emission=(0.95, 0.65, 0.15), emission_strength=1.0)
    pupil = material("Pupil", (0.02, 0.02, 0.02), 0.3)
    nose = material("Nose", (0.9, 0.55, 0.55), 0.5)
    fang = material("Fang", (0.96, 0.95, 0.9), 0.35)
    body = Blob("Body", orange, 0.05)
    body.ball((0.0, 0.6, -1.3), 1.45, (1.2, 0.9, 1.0), axis=(1, 0, 0))      # chest
    body.ball((0.0, 0.3, -0.35), 0.85)                                       # neck
    body.ball((0.0, 0.0, 0.55), 1.05, (1.05, 0.95, 1.0), axis=(1, 0, 0))    # head
    body.ball((0.0, -0.55, 0.15), 0.72, (1.0, 0.85, 0.7), axis=(1, 0, 0))   # muzzle mass
    for s in (-1, 1):
        body.ball((s * 1.15, 0.45, -0.85), 0.6)                              # shoulders
        body.ball((s * 0.72, 0.1, 1.25), 0.34)                               # ears
    # white muzzle, chin and eye patches
    sphere("Muzzle", (0.0, -0.95, 0.05), (0.62, 0.32, 0.42), white)
    sphere("Chin", (0.0, -0.8, -0.45), (0.5, 0.3, 0.25), white)
    for s in (-1, 1):
        sphere("Cheek%d" % s, (s * 0.7, -0.6, 0.35), (0.32, 0.2, 0.22), white)
        sphere("Brow%d" % s, (s * 0.42, -0.7, 0.78), (0.2, 0.12, 0.1), white)
        sphere("Eye%d" % s, (s * 0.42, -0.86, 0.66), (0.17, 0.1, 0.14), eye)
        sphere("Pupil%d" % s, (s * 0.42, -0.97, 0.66), (0.05, 0.04, 0.1), pupil)
        sphere("EarIn%d" % s, (s * 0.72, -0.05, 1.3), (0.2, 0.1, 0.24), white)
    sphere("Nose", (0.0, -1.22, 0.22), (0.2, 0.1, 0.12), nose)
    # stripes: dark bands over the brow, cheeks, neck and shoulders
    for k in range(5):
        y = 0.9 + 0.12 * (k % 2)
        cone_along("BrowStripe%d" % k, (-0.6 + 0.3 * k, -0.35, y), (0.15, 0.3, 0.55), 0.55, 0.075, dark, 0.02)
    for s in (-1, 1):
        for k in range(4):
            cone_along("CheekStripe%d%d" % (s, k), (s * (0.75 + 0.15 * k), -0.35 + 0.1 * k, 0.55 - 0.3 * k),
                       (s * 0.35, -0.3, -0.9), 0.55, 0.07, dark, 0.02)
        for k in range(5):
            cone_along("NeckStripe%d%d" % (s, k), (s * (0.5 + 0.28 * k), 0.15 + 0.05 * k, -0.25 - 0.25 * k),
                       (s * 0.4, 0.2, -1.0), 0.8, 0.09, dark, 0.03)
    # fangs and mouth line
    cone_along("MouthLine", (-0.35, -1.18, -0.05), (1.0, 0.0, 0.0), 0.7, 0.03, pupil, 0.03)
    for s in (-1, 1):
        cone_along("Fang%d" % s, (s * 0.24, -1.2, -0.05), (0.0, -0.05, -1.0), 0.32, 0.06, fang)
        cone_along("Whisker%d%d" % (s, 0), (s * 0.3, -1.1, 0.0), (s * 1.0, -0.1, -0.15), 1.1, 0.012, white)
        cone_along("Whisker%d%d" % (s, 1), (s * 0.3, -1.05, -0.1), (s * 1.0, -0.1, -0.4), 1.0, 0.012, white)


def shadow_mastiff():
    """A shadow mastiff's head and shoulders facing the viewer: a heavy hound
    built out of darkness, with ears carried high, a broad blunt muzzle, and
    pale blue eyes that are the only bright thing on it.

    Everything about the coat is near-black, because that is the creature: a
    black dog reads as a dog, and a dog made of shadow has to read as shadow.
    The whole portrait therefore rests on two cues the eye can still find at
    128 pixels - the silhouette, and the eyes.

    2026-10-02 art repair. The first build dressed the head with separate pale
    plates for the crown, the brow and the withers, which do not merge with the
    metaball body and so rendered as discs lying on the face; the eyes were
    near-white and strongly emissive above those plates, which read as cartoon
    eyes; the ears were long thin ellipsoids swept along the depth axis, which
    read as a rabbit's; and the open maw was a single large red sphere, which
    read as a tongue hanging off the chin. The dressing is gone, the coat's own
    procedural noise does that work instead, and the eyes, ears and muzzle are
    all brought back towards the proportions the Tiger portrait on this same
    rig family already uses.
    """
    coat = material("Coat", (0.050, 0.052, 0.068), 0.82,
                    noise=(5.0, 0.5, (0.018, 0.020, 0.032)))
    under = material("Under", (0.022, 0.024, 0.034), 0.86)
    eye = material("Eye", (0.22, 0.38, 0.78), 0.18,
                   emission=(0.20, 0.40, 0.95), emission_strength=0.55)
    maw = material("Maw", (0.10, 0.04, 0.05), 0.75)
    fang = material("Fang", (0.86, 0.87, 0.90), 0.3)
    body = Blob("Body", coat, 0.05)
    body.ball((0.0, 0.62, -1.35), 1.42, (1.25, 0.88, 0.95), axis=(1, 0, 0))   # chest
    body.ball((0.0, 0.26, -0.40), 0.80)                                       # neck
    body.ball((0.0, 0.0, 0.52), 1.00, (1.02, 0.94, 1.0), axis=(1, 0, 0))      # skull
    for s in (-1, 1):
        body.ball((s * 0.44, -0.18, 0.26), 0.44)                              # jowls
        body.ball((s * 0.40, -0.50, 0.72), 0.30)                              # brow ridge
    body.ball((0.0, -0.70, 0.14), 0.58, (1.0, 0.90, 0.74), axis=(1, 0, 0))    # muzzle
    body.ball((0.0, -1.14, 0.06), 0.40, (1.0, 0.92, 0.72), axis=(1, 0, 0))    # snout
    for s in (-1, 1):
        body.ball((s * 1.14, 0.48, -0.92), 0.60)                              # shoulders
        # Carried high and a little back, broad at the base: a hound's ear,
        # not the tall blade a wolf gets or the thin paddle the first build had.
        body.ball((s * 0.66, 0.16, 1.20), 0.33, (0.58, 0.92, 1.18),
                  axis=(0, 0, 1))
    # A dark mouth line drawn across the muzzle rather than a sphere hung
    # under it: the first repair still read as a tongue from the front.
    cone_along("Jaw", (0.0, -0.52, -0.36), (0.0, -1.0, -0.10), 0.62, 0.28,
               coat, 0.12)
    # Seated into the muzzle and in the same near-black the underside uses:
    # drawn in the maw colour and standing proud of the face, it read as a
    # stick held crosswise in the creature's mouth.
    cone_along("MouthLine", (-0.26, -1.08, -0.14), (1.0, 0.0, 0.0), 0.52,
               0.030, under, 0.030)
    for s in (-1, 1):
        cone_along("Fang%d" % s, (s * 0.14, -1.07, -0.15), (0.0, -0.06, -1.0),
                   0.15, 0.036, fang)
        sphere("Eye%d" % s, (s * 0.38, -0.80, 0.56), (0.105, 0.07, 0.09), eye)
        sphere("EarIn%d" % s, (s * 0.66, -0.06, 1.18), (0.16, 0.08, 0.52),
               under)
    sphere("Nose", (0.0, -1.38, 0.10), (0.18, 0.11, 0.11), under)


def giant_ant(soldier, drone=False):
    """A giant ant in profile: head, pinched waist and gaster, three masses on
    a thread, which is the only thing that names an ant at 64 pixels.

    The first build framed it head-on and close, so all three masses hid behind
    the face and what rendered was a brown ball with sticks. The creature is
    now laid along the view's horizontal, slightly turned, so the silhouette
    does the work and the mandibles and elbowed antennae decorate it rather
    than carry it.

    The caste is the same three differences the mesh uses - head size, mandible
    weight, and whether a sting shows - because a player who can tell them apart
    in the field should be able to tell them apart in the menu.

    Sprint 15's Drone is a fourth difference on the same build: it is the
    soldier with the advanced simple template, so it keeps the soldier's heavy
    head, mandibles and sting and adds two pairs of wings folded back over the
    gaster. The wings are what a player will actually use to tell it from the
    soldier at icon size, so they are raised off the body and lit pale against
    the dark chitin rather than laid flat where the silhouette would swallow
    them.
    """
    head_scale = 1.0 if soldier else 0.78
    chitin = material("Chitin", (0.172, 0.084, 0.047), 0.32,
                      noise=(7.0, 0.42, (0.078, 0.035, 0.021)))
    limb = material("Limb", (0.132, 0.063, 0.036), 0.40)
    jaw = material("Jaw", (0.050, 0.027, 0.017), 0.28)
    eye = material("AntEye", (0.028, 0.024, 0.022), 0.10)
    body = Blob("Body", chitin, 0.045)
    head = (-1.92, 0.0, 0.52)
    body.ball(head, 0.80 * head_scale, (1.04, 0.96, 0.98), axis=(1, 0, 0))
    body.ball((-1.02, 0.0, 0.34), 0.26)                                  # neck
    body.ball((-0.42, 0.0, 0.50), 0.62, (1.28, 0.90, 0.92), axis=(1, 0, 0))
    body.ball((0.34, 0.0, 0.28), 0.15)                                   # petiole
    body.ball((1.28, 0.0, 0.52), 0.82, (1.24, 0.92, 0.94), axis=(1, 0, 0))
    for s in (-1, 1):
        # Compound eyes on the head's sides, set well forward.
        sphere("Eye%d" % s, (-2.16, s * 0.52 * head_scale, 0.62),
               (0.15, 0.11, 0.13), eye)
        # Mandibles: out and forward, then converging. Two segments, because a
        # straight cone reads as a tusk.
        heavy = 0.135 if soldier else 0.078
        cone_along("Mandible%dA" % s, (-2.42, s * 0.34 * head_scale, 0.18),
                   (-1.0, s * 0.42, -0.06), 0.78 * head_scale, heavy, jaw,
                   heavy * 0.74)
        cone_along("Mandible%dB" % s,
                   (-3.12, s * 0.62 * head_scale, 0.12),
                   (-1.0, -s * 0.66, 0.10), 0.62 * head_scale, heavy * 0.72,
                   jaw, heavy * 0.14)
        # The elbowed antenna: a scape up and forward, then a funiculus out.
        cone_along("Scape%d" % s, (-2.08, s * 0.34, 1.04),
                   (-0.42, s * 0.30, 1.0), 0.94, 0.068, limb, 0.050)
        cone_along("Funiculus%d" % s, (-2.48, s * 0.62, 1.92),
                   (-1.0, s * 0.18, 0.22), 1.26, 0.050, limb, 0.020)
        # Three pairs of legs from the mesosoma, each a femur out and up and a
        # tibia down to the ground line.
        for index, (root, out) in enumerate((
                (-0.92, -0.46), (-0.38, 0.0), (0.10, 0.46))):
            cone_along("Femur%d%d" % (s, index), (root, s * 0.22, 0.42),
                       (out, s * 1.0, 0.52), 0.92, 0.096, limb, 0.062)
            cone_along("Tibia%d%d" % (s, index),
                       (root + out * 0.55, s * 0.86, 0.90),
                       (out * 0.6, s * 0.55, -1.0), 1.46, 0.058, limb, 0.020)
    if soldier:
        # The sting. The one feature a worker does not have, so it is worth the
        # two primitives even at this size.
        cone_along("Sting", (2.08, 0.0, 0.34), (1.0, 0.0, -0.46), 0.62,
                   0.070, jaw, 0.012)
    if drone:
        # Two pairs of wings, the forewing long and the hindwing short, swept
        # back over the gaster and tilted up so each pair reads separately
        # against the body instead of merging into one shape.
        membrane = material("Wing", (0.70, 0.58, 0.36), 0.16,
                            emission=(0.50, 0.40, 0.23),
                            emission_strength=0.45)
        for s in (-1, 1):
            # Broad across, thin through, and sitting down on the mesosoma
            # rather than hovering over it. The first build made them narrow
            # as well as thin, so each rendered as a sliver that read like a
            # scratch across the frame instead of a wing.
            fore = Blob("Forewing%d" % s, membrane, 0.040)
            fore.ball((-0.24, s * 0.34, 0.96), 0.44, (1.70, 1.25, 0.16),
                      axis=(1, 0, 0))
            fore.ball((0.86, s * 0.66, 1.02), 0.46, (1.95, 1.30, 0.15),
                      axis=(1, 0, 0))
            fore.ball((2.00, s * 0.92, 1.04), 0.34, (1.70, 1.05, 0.13),
                      axis=(1, 0, 0))
            hind = Blob("Hindwing%d" % s, membrane, 0.040)
            hind.ball((0.10, s * 0.52, 0.80), 0.34, (1.35, 1.05, 0.14),
                      axis=(1, 0, 0))
            hind.ball((1.05, s * 0.76, 0.84), 0.30, (1.25, 0.95, 0.12),
                      axis=(1, 0, 0))


def fire_beetle():
    """A fire beetle, three-quarter and from slightly above.

    The glands are the portrait. The source gives this creature luminescence
    and no fire damage at all, so the one bright thing in the frame is a pair
    of emissive glands on its head, and everything else - a near-black shell
    with a red cast, short legs, clubbed antennae - exists so that they have
    something to glow against.

    The first build made the glands large and nearly white, which read as
    cartoon eyes, and the shell too pale for them to tell against. They are
    smaller and redder now, the shell is near-black, and the elytra carry a
    visible suture so the back is a beetle's and not a dome.
    """
    shell = material("Shell", (0.058, 0.026, 0.022), 0.26,
                     noise=(6.0, 0.40, (0.026, 0.011, 0.010)))
    elytron = material("Elytron", (0.104, 0.040, 0.030), 0.20,
                       noise=(16.0, 0.45, (0.038, 0.014, 0.012)))
    limb = material("BeetleLimb", (0.048, 0.021, 0.018), 0.42)
    gland = material("Gland", (1.0, 0.34, 0.08), 0.16,
                     emission=(1.0, 0.36, 0.08), emission_strength=5.0)
    body = Blob("Body", shell, 0.045)
    body.ball((0.0, -1.46, 0.10), 0.54, (1.00, 1.18, 0.72), axis=(0, 1, 0))
    body.ball((0.0, -0.62, 0.26), 0.98, (1.12, 1.30, 0.76), axis=(0, 1, 0))
    body.ball((0.0, 0.86, 0.22), 1.14, (1.42, 1.44, 0.80), axis=(0, 1, 0))
    for s in (-1, 1):
        # One elytron a side, raised and offset so the suture between the pair
        # is visible from this angle.
        wing = Blob("Elytron%d" % s, elytron, 0.045)
        wing.ball((s * 0.46, 0.10, 0.84), 0.50, (0.82, 1.52, 0.42),
                  axis=(0, 1, 0))
        wing.ball((s * 0.44, 1.10, 0.78), 0.48, (0.80, 1.44, 0.40),
                  axis=(0, 1, 0))
        wing.ball((s * 0.34, 1.92, 0.62), 0.38, (0.70, 1.10, 0.34),
                  axis=(0, 1, 0))
        # The glands, above the eyes, where the source puts them. Small enough
        # to read as lights rather than as a face.
        sphere("Gland%d" % s, (s * 0.34, -1.80, 0.46), (0.155, 0.135, 0.145),
               gland)
        cone_along("Leg%dA" % s, (s * 0.58, -0.72, -0.20),
                   (s * 1.0, -0.50, 0.20), 0.88, 0.094, limb, 0.060)
        cone_along("Leg%dB" % s, (s * 1.38, -1.14, -0.02),
                   (s * 0.38, -0.28, -1.0), 1.04, 0.054, limb, 0.020)
        cone_along("Leg%dC" % s, (s * 0.68, 0.42, -0.26),
                   (s * 1.0, 0.28, 0.14), 0.86, 0.088, limb, 0.056)
        cone_along("Leg%dD" % s, (s * 1.48, 0.68, -0.14),
                   (s * 0.32, 0.24, -1.0), 1.00, 0.050, limb, 0.018)
        # Clubbed antennae: short, and thicker at the tip than the base.
        cone_along("Antenna%d" % s, (s * 0.40, -1.86, 0.22),
                   (s * 0.62, -1.0, 0.46), 0.92, 0.050, limb, 0.070)


def giant_stag_beetle():
    """A giant stag beetle in profile, head to the left.

    The mandibles are the portrait, so the creature is laid along the view's
    horizontal exactly as the ant is. The first build copied the fire beetle
    and pointed the head at the camera; the antlers foreshortened straight into
    the body and the rendered frame was a brown blob with legs, missing the one
    feature that names the creature. A forward-projecting feature needs a
    profile to live in.

    Everything else says heavy. This is a Large creature with a 20-foot speed
    that bowls over anything smaller than itself, so the body is long and
    deep, the elytra are broad with a visible suture, and the legs are short
    and thick rather than the ant's thin struts.
    """
    shell = material("StagShell", (0.098, 0.058, 0.030), 0.30,
                     noise=(5.0, 0.44, (0.044, 0.025, 0.013)))
    elytron = material("StagElytron", (0.150, 0.088, 0.045), 0.18,
                       noise=(13.0, 0.40, (0.064, 0.036, 0.018)))
    limb = material("StagLimb", (0.074, 0.043, 0.022), 0.40)
    antler = material("Antler", (0.206, 0.126, 0.060), 0.22,
                      noise=(9.0, 0.30, (0.092, 0.055, 0.026)))
    eye = material("StagEye", (0.030, 0.026, 0.024), 0.10)
    body = Blob("Body", shell, 0.045)
    # Head, a short thick neck, a deep thorax and a long broad abdomen.
    body.ball((-1.66, 0.0, 0.46), 0.60, (1.06, 1.00, 0.86), axis=(1, 0, 0))
    body.ball((-0.96, 0.0, 0.42), 0.30)
    body.ball((-0.30, 0.0, 0.52), 0.76, (1.22, 1.08, 0.94), axis=(1, 0, 0))
    body.ball((1.00, 0.0, 0.50), 0.96, (1.46, 1.16, 0.90), axis=(1, 0, 0))
    for s in (-1, 1):
        # Elytra along the abdomen, raised so the suture between them reads.
        wing = Blob("StagElytron%d" % s, elytron, 0.045)
        wing.ball((0.10, s * 0.38, 1.02), 0.44, (1.30, 0.82, 0.34),
                  axis=(1, 0, 0))
        wing.ball((1.10, s * 0.42, 1.02), 0.46, (1.40, 0.86, 0.34),
                  axis=(1, 0, 0))
        wing.ball((2.00, s * 0.32, 0.88), 0.34, (1.10, 0.66, 0.28),
                  axis=(1, 0, 0))
        sphere("StagEye%d" % s, (-1.92, s * 0.44, 0.60),
               (0.14, 0.11, 0.13), eye)
        # The antlers: a heavy base forward and out, a longer arm curving in,
        # and one inner tine. Held wide enough that the gap between them is
        # part of the silhouette.
        cone_along("Antler%dA" % s, (-2.06, s * 0.34, 0.46),
                   (-1.0, s * 0.30, 0.16), 1.16, 0.150, antler, 0.108)
        cone_along("Antler%dB" % s, (-3.10, s * 0.66, 0.66),
                   (-1.0, -s * 0.46, 0.04), 1.00, 0.102, antler, 0.030)
        cone_along("Tine%d" % s, (-2.86, s * 0.58, 0.60),
                   (-0.30, -s * 1.0, 0.26), 0.58, 0.060, antler, 0.014)
        # Six short braced legs: a thick femur out and a tibia to the ground.
        for index, (root, out) in enumerate((
                (-0.74, -0.40), (-0.20, 0.0), (0.38, 0.40))):
            cone_along("StagFemur%d%d" % (s, index), (root, s * 0.30, 0.34),
                       (out, s * 1.0, 0.30), 0.86, 0.128, limb, 0.086)
            cone_along("StagTibia%d%d" % (s, index),
                       (root + out * 0.5, s * 0.92, 0.66),
                       (out * 0.4, s * 0.42, -1.0), 1.18, 0.082, limb, 0.030)


def dire_crocodile():
    """A dire crocodile in profile, head to the left.

    Profile, not three-quarter, and for the same reason the stag beetle needed
    one: the feature that names this creature projects forward. A crocodile
    read down its own length is a brown wedge; read across, it is a jaw. The
    camera sits low as well, because a crocodile's whole silhouette is that it
    is long and flat and close to the ground, and anything looking down at it
    turns that into an oval.

    The sprint's order says no lizard silhouette disguised by texture, so the
    things that separate the two are built rather than painted: a broad flat
    snout instead of a tapering muzzle, a jaw line long enough to carry teeth
    along it, raised brow and nostril bosses, armoured dorsal scutes in two
    ranks, a tail that is deep and keeled rather than round, and short limbs
    splayed out to the sides instead of tucked under the body.
    """
    hide = material("CrocHide", (0.062, 0.070, 0.048), 0.42,
                    noise=(7.0, 0.46, (0.030, 0.036, 0.024)))
    belly = material("CrocBelly", (0.146, 0.138, 0.104), 0.52,
                     noise=(16.0, 0.30, (0.092, 0.088, 0.066)))
    scute = material("CrocScute", (0.044, 0.050, 0.034), 0.30,
                     noise=(11.0, 0.40, (0.082, 0.090, 0.058)))
    limb = material("CrocLimb", (0.052, 0.058, 0.040), 0.46)
    tooth = material("CrocTooth", (0.184, 0.176, 0.150), 0.24)
    # A low eyeshine, which a crocodile actually has and which gives the
    # frame one bright point at 128 pixels.
    eye = material("CrocEye", (0.230, 0.146, 0.040), 0.16,
                   emission=(0.230, 0.146, 0.040),
                   emission_strength=0.30)
    body = Blob("Body", hide, 0.045)
    # Snout, skull, shoulders, trunk and hips, all within about five units so
    # the whole animal sits inside the ring. The snout balls are flattened
    # hard in z, because a crocodile's head is wide and low and a round one is
    # a lizard.
    body.ball((-2.34, 0.0, 0.34), 0.34, (1.44, 0.96, 0.46), axis=(1, 0, 0))
    body.ball((-1.80, 0.0, 0.37), 0.42, (1.34, 1.04, 0.54), axis=(1, 0, 0))
    body.ball((-1.22, 0.0, 0.44), 0.54, (1.18, 1.16, 0.70), axis=(1, 0, 0))
    body.ball((-0.52, 0.0, 0.48), 0.68, (1.20, 1.20, 0.82), axis=(1, 0, 0))
    body.ball((0.34, 0.0, 0.50), 0.74, (1.30, 1.22, 0.86), axis=(1, 0, 0))
    body.ball((1.22, 0.0, 0.46), 0.62, (1.28, 1.04, 0.78), axis=(1, 0, 0))
    # One continuous tail rather than four lumps: a chain fuses because its
    # elements overlap, and it stops well inside the frame.
    body.chain((1.70, 0.04, 0.44), (3.15, 0.26, 0.40), 0.46, 0.15, count=9)
    # A paler belly slab, which is what makes the body read as a heavy
    # cylinder lying on something rather than as a silhouette.
    under = Blob("CrocBelly", belly, 0.050)
    under.ball((-0.40, 0.0, 0.12), 0.40, (1.50, 1.04, 0.30), axis=(1, 0, 0))
    under.ball((0.45, 0.0, 0.12), 0.44, (1.50, 1.04, 0.30), axis=(1, 0, 0))
    # The lower jaw, slung under the snout and slightly open. An open jaw is
    # what makes the portrait a predator rather than a log.
    jaw = Blob("CrocJaw", hide, 0.042)
    jaw.ball((-2.24, 0.0, 0.06), 0.24, (1.34, 0.80, 0.34), axis=(1, 0, 0))
    jaw.ball((-1.72, 0.0, 0.12), 0.27, (1.22, 0.88, 0.38), axis=(1, 0, 0))
    jaw.ball((-1.24, 0.0, 0.20), 0.31, (1.08, 0.94, 0.44), axis=(1, 0, 0))
    for s in (-1, 1):
        # A raised brow with the eye on its side rather than on top, so the
        # near one carries the frame and the far one stays a hint.
        sphere("CrocBrow%d" % s, (-1.20, s * 0.26, 0.64),
               (0.22, 0.17, 0.13), hide)
        sphere("CrocEye%d" % s, (-1.22, s * 0.37, 0.60),
               (0.12, 0.09, 0.10), eye)
        sphere("CrocNostril%d" % s, (-2.52, s * 0.12, 0.46),
               (0.10, 0.08, 0.07), hide)
        # Teeth along both jaw lines, the front ones longest the way a
        # crocodile's fourth tooth is, and short enough not to read as a comb.
        for index, (x, length) in enumerate((
                (-2.38, 0.20), (-2.06, 0.16), (-1.74, 0.17), (-1.42, 0.13))):
            cone_along("CrocToothUpper%d%d" % (s, index),
                       (x, s * 0.19, 0.16), (0.0, 0.0, -1.0), length,
                       0.046, tooth, 0.004)
            cone_along("CrocToothLower%d%d" % (s, index),
                       (x + 0.15, s * 0.17, 0.00), (0.0, 0.0, 1.0),
                       length * 0.75, 0.040, tooth, 0.004)
        # Low keeled scutes hugging the spine: plates, not spikes. Flattened
        # spheres rather than cones, because the first build's cones made the
        # back read as a stegosaurus.
        for index, (x, scale) in enumerate((
                (-0.75, 0.92), (-0.25, 1.00), (0.25, 1.02), (0.75, 0.96),
                (1.25, 0.84), (1.75, 0.68), (2.25, 0.52))):
            sphere("CrocScute%d%d" % (s, index),
                   (x, s * 0.20, 0.80 + 0.04 * scale),
                   (0.20 * scale, 0.13 * scale, 0.09 * scale), scute)
            if index < 5:
                sphere("CrocScuteOuter%d%d" % (s, index),
                       (x + 0.06, s * 0.37, 0.73),
                       (0.17 * scale, 0.11 * scale, 0.07 * scale), scute)
        # Four short sprawling legs, thick and rooted inside the hull, with a
        # wide foot. Sprawl is half of why a crocodile looks like a crocodile.
        for index, (root, out) in enumerate(((-0.70, -0.26), (1.05, 0.24))):
            cone_along("CrocHumerus%d%d" % (s, index),
                       (root, s * 0.26, 0.34), (out, s * 1.0, -0.42), 0.60,
                       0.185, limb, 0.140)
            cone_along("CrocForearm%d%d" % (s, index),
                       (root + out * 0.46, s * 0.80, 0.08),
                       (out * 0.4, s * 0.30, -1.0), 0.34, 0.132, limb, 0.098)
            box("CrocFoot%d%d" % (s, index),
                (root + out * 0.66, s * 0.93, -0.12),
                (0.26, 0.20, 0.07), limb, bevel=0.025)


CREATURES = {
    # The two ant castes share a backdrop and a light rig because they are one
    # creature in two builds; what separates them in the frame is the head and
    # the mandibles, which is what separates them in the stat block.
    "giant-ant-soldier": dict(build=lambda: giant_ant(True),
                     inner=(0.26, 0.13, 0.06), outer=(0.028, 0.014, 0.008),
                     key=(1.0, 0.86, 0.60), rim=(0.70, 0.80, 1.0),
                     camera=((1.5, -9.8, 1.95), (-0.30, 0.0, 0.46), 46.0)),
    "giant-ant-worker": dict(build=lambda: giant_ant(False),
                     inner=(0.26, 0.13, 0.06), outer=(0.028, 0.014, 0.008),
                     key=(1.0, 0.86, 0.60), rim=(0.70, 0.80, 1.0),
                     camera=((1.5, -9.8, 1.95), (-0.30, 0.0, 0.46), 46.0)),
    # The Drone shares the castes' backdrop and rig for the same reason, and
    # pulls the camera back a little because the wings widen the subject.
    "giant-ant-drone": dict(build=lambda: giant_ant(True, True),
                     inner=(0.26, 0.13, 0.06), outer=(0.028, 0.014, 0.008),
                     key=(1.0, 0.86, 0.60), rim=(0.70, 0.80, 1.0),
                     camera=((1.6, -10.6, 2.30), (-0.30, 0.0, 0.58), 46.0)),
    # A heavy ground beetle whose portrait is its mandibles: a cool slate
    # backdrop so the warm antlers carry against it, and a camera low enough
    # that the body reads as mass rather than as a disc from above.
    # The crocodilians: a cool, damp, green-grey backdrop rather than the
    # insects' warm brown, so a dark olive animal separates from it, and a low
    # camera well back along the body because the subject is long rather than
    # tall. The Dire Crocodile is Gargantuan, but an icon is a portrait and
    # size lives in the stat block, so what the frame has to carry is the jaw.
    "dire-crocodile": dict(build=dire_crocodile,
                     inner=(0.088, 0.128, 0.104), outer=(0.012, 0.020, 0.016),
                     key=(1.0, 0.90, 0.68), rim=(0.58, 0.82, 0.92),
                     camera=((1.4, -8.6, 1.32), (0.12, 0.0, 0.40), 46.0)),
    "giant-stag-beetle": dict(build=giant_stag_beetle,
                     inner=(0.11, 0.12, 0.15), outer=(0.010, 0.011, 0.014),
                     key=(1.0, 0.90, 0.70), rim=(0.66, 0.76, 1.0),
                     camera=((1.5, -10.4, 2.10), (-0.40, 0.0, 0.56), 46.0)),
    # A near-black creature whose whole identity is that it glows: the backdrop
    # is deep and cool so the two emissive glands are the only warm thing
    # anywhere in the frame.
    "fire-beetle": dict(build=fire_beetle,
                     inner=(0.09, 0.07, 0.11), outer=(0.008, 0.006, 0.010),
                     key=(0.80, 0.78, 0.92), rim=(0.62, 0.70, 1.0),
                     camera=((0.45, -7.4, 1.90), (0.0, 0.05, 0.16), 54.0)),
    "pony": dict(build=lambda: equine(True),
                 inner=(0.5, 0.38, 0.16), outer=(0.05, 0.04, 0.025),
                 key=(1.0, 0.88, 0.62), rim=(0.85, 0.85, 0.95),
                 camera=((1.2, -6.8, 1.35), (-0.35, -0.1, 0.75), 60.0)),
    "horse": dict(build=lambda: equine(False),
                  inner=(0.45, 0.26, 0.11), outer=(0.035, 0.025, 0.035),
                  key=(1.0, 0.82, 0.55), rim=(0.85, 0.8, 0.9),
                  camera=((1.3, -7.4, 1.6), (-0.4, -0.1, 0.95), 60.0)),
    "owlbear": dict(build=owlbear,
                    inner=(0.14, 0.28, 0.12), outer=(0.015, 0.025, 0.015),
                    key=(1.0, 0.86, 0.6), rim=(0.5, 0.8, 1.0),
                    camera=((0.0, -8.4, 1.3), (0.0, 0.0, 0.5), 55.0)),
    "cyclops": dict(build=cyclops,
                    inner=(0.24, 0.28, 0.34), outer=(0.015, 0.015, 0.025),
                    key=(1.0, 0.8, 0.55), rim=(0.55, 0.7, 1.0),
                    camera=((0.4, -7.8, 0.3), (0.35, 0.0, 0.7), 55.0)),
    "shambling-mound": dict(build=shambling_mound,
                            inner=(0.10, 0.20, 0.10), outer=(0.012, 0.02, 0.012),
                            key=(0.95, 0.9, 0.7), rim=(0.5, 0.85, 0.7),
                            camera=((0.0, -8.8, 1.3), (0.0, 0.0, 0.3), 55.0)),
    "giant-flytrap": dict(build=giant_flytrap,
                          inner=(0.08, 0.2, 0.16), outer=(0.01, 0.02, 0.018),
                          key=(0.95, 0.92, 0.75), rim=(0.45, 0.85, 0.75),
                          camera=((0.0, -9.0, 1.0), (0.0, 0.0, 0.1), 52.0)),
    "purple-worm": dict(build=purple_worm,
                        inner=(0.22, 0.12, 0.3), outer=(0.02, 0.012, 0.03),
                        key=(1.0, 0.85, 0.7), rim=(0.6, 0.5, 1.0),
                        camera=((0.6, -9.0, 1.1), (0.7, 0.0, 0.0), 55.0)),
    "dust-mephit": dict(build=lambda: mephit("dust"),
                     inner=MEPHIT_DRESSINGS["dust"]["inner"], outer=MEPHIT_DRESSINGS["dust"]["outer"],
                     key=MEPHIT_DRESSINGS["dust"]["key"], rim=MEPHIT_DRESSINGS["dust"]["rim"],
                     camera=((0.3, -7.4, 0.9), (-0.1, 0.0, 0.1), 55.0)),
    "ice-mephit": dict(build=lambda: mephit("ice"),
                     inner=MEPHIT_DRESSINGS["ice"]["inner"], outer=MEPHIT_DRESSINGS["ice"]["outer"],
                     key=MEPHIT_DRESSINGS["ice"]["key"], rim=MEPHIT_DRESSINGS["ice"]["rim"],
                     camera=((0.3, -7.4, 0.9), (-0.1, 0.0, 0.1), 55.0)),
    "magma-mephit": dict(build=lambda: mephit("magma"),
                     inner=MEPHIT_DRESSINGS["magma"]["inner"], outer=MEPHIT_DRESSINGS["magma"]["outer"],
                     key=MEPHIT_DRESSINGS["magma"]["key"], rim=MEPHIT_DRESSINGS["magma"]["rim"],
                     camera=((0.3, -7.4, 0.9), (-0.1, 0.0, 0.1), 55.0)),
    "ooze-mephit": dict(build=lambda: mephit("ooze"),
                     inner=MEPHIT_DRESSINGS["ooze"]["inner"], outer=MEPHIT_DRESSINGS["ooze"]["outer"],
                     key=MEPHIT_DRESSINGS["ooze"]["key"], rim=MEPHIT_DRESSINGS["ooze"]["rim"],
                     camera=((0.3, -7.4, 0.9), (-0.1, 0.0, 0.1), 55.0)),
    "salt-mephit": dict(build=lambda: mephit("salt"),
                     inner=MEPHIT_DRESSINGS["salt"]["inner"], outer=MEPHIT_DRESSINGS["salt"]["outer"],
                     key=MEPHIT_DRESSINGS["salt"]["key"], rim=MEPHIT_DRESSINGS["salt"]["rim"],
                     camera=((0.3, -7.4, 0.9), (-0.1, 0.0, 0.1), 55.0)),
    "tiger": dict(build=tiger,
                  inner=(0.3, 0.16, 0.05), outer=(0.03, 0.015, 0.005),
                  key=(1.0, 0.9, 0.75), rim=(1.0, 0.7, 0.4),
                  camera=((0.3, -7.6, 0.9), (-0.1, 0.0, 0.05), 55.0)),
    # A near-black creature needs a backdrop it can be seen against, so
    # this one is the coldest and deepest in the roster and its rim light
    # is the brightest: the silhouette is carried by the rim, not by the
    # coat.
    "shadow-mastiff": dict(build=shadow_mastiff,
                     inner=(0.10, 0.13, 0.24), outer=(0.006, 0.008, 0.016),
                     key=(0.72, 0.78, 1.0), rim=(0.55, 0.72, 1.0),
                     camera=((0.25, -7.6, 0.95), (-0.05, 0.0, 0.1), 55.0)),
    "steam-mephit": dict(build=lambda: mephit("steam"),
                     inner=MEPHIT_DRESSINGS["steam"]["inner"], outer=MEPHIT_DRESSINGS["steam"]["outer"],
                     key=MEPHIT_DRESSINGS["steam"]["key"], rim=MEPHIT_DRESSINGS["steam"]["rim"],
                     camera=((0.3, -7.4, 0.9), (-0.1, 0.0, 0.1), 55.0)),
    # Sprint 18. The gorilla is nearly black, so like the Shadow Mastiff its
    # silhouette is carried by a cool rim against a deep cold backdrop; the
    # gigantopithecus is a warm reddish brute and takes a warm key against cold
    # jungle green, so the two are never one dressing used twice.
    "ape": dict(build=lambda: ape(False),
                inner=(0.10, 0.12, 0.15), outer=(0.008, 0.010, 0.013),
                key=(0.80, 0.84, 0.95), rim=(0.60, 0.78, 1.0),
                camera=((0.30, -7.8, 0.55), (-0.03, 0.0, -0.05), 55.0)),
    "dire-ape": dict(build=lambda: ape(True),
                     inner=(0.09, 0.15, 0.10), outer=(0.010, 0.016, 0.010),
                     key=(1.0, 0.86, 0.62), rim=(0.70, 0.95, 0.80),
                     camera=((0.42, -8.1, 0.50), (-0.03, 0.0, -0.08), 55.0)),
    # Sprint 19. A nearly white girallon needs a dark warm ground and a cool
    # key, which is the inverse of the near-black gorilla's dressing; the xill
    # is a hot orange-red and takes a cold key against deep green-black, so
    # neither Sprint 19 creature reuses a Sprint 18 dressing.
    "girallon": dict(build=girallon,
                     inner=(0.14, 0.10, 0.08), outer=(0.016, 0.010, 0.007),
                     key=(0.86, 0.90, 1.0), rim=(0.65, 0.80, 1.0),
                     camera=((0.36, -8.4, 0.70), (-0.03, 0.0, 0.05), 55.0)),
    "xill": dict(build=xill,
                 inner=(0.07, 0.12, 0.09), outer=(0.006, 0.012, 0.009),
                 key=(0.78, 0.88, 1.0), rim=(0.45, 0.95, 0.70),
                 camera=((0.30, -8.0, 0.62), (-0.02, 0.0, 0.02), 55.0)),
}


def main():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--creature", required=True, choices=sorted(CREATURES))
    parser.add_argument("--out", required=True)
    parser.add_argument("--samples", type=int, default=128)
    args = parser.parse_args(argv)
    spec = CREATURES[args.creature]
    scene = clear_scene()
    scene.cycles.samples = args.samples
    location, target, lens = spec["camera"]
    backdrop(spec["inner"], spec["outer"], location, target, lens)
    lights(spec["key"], spec["rim"])
    camera(location, target, lens)
    spec["build"]()
    scene.render.filepath = args.out
    bpy.ops.render.render(write_still=True)
    print("rendered", args.creature, "->", args.out)


if __name__ == "__main__":
    main()
